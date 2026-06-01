using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using UnityEngine;
using Random = System.Random;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class LevelGenerator {
        public struct GenerationParams {
            public int MinRings, MaxRings;
            public int MinAreas, MaxAreas;
            public int MinSectors, MaxSectors;
            public int MinAreaSpan, MaxAreaSpan;
            public bool AllowBlocked;
            public float BlockedChance;
            public bool AllowFilterColors;
            public float FilterColorsChance;
            public int MinFilterColors, MaxFilterColors;
        }

        public class RawLevelData {
            public int Rings;
            public int Sectors;
            public byte[,] Colors;
            public SegmentStatus[,] Statuses;
            public List<SlideAreaConfig> Areas;
            public int Difficulty;
        }

        public async Task<RawLevelData> GenerateAsync(GenerationParams p, int targetDifficulty, System.Threading.CancellationToken ct, IProgress<string> progress = null) {
            return await Task.Run(() => {
                var rnd = new Random();
                int maxAttempts = 10;
                RawLevelData bestLevel = null;

                for (int attempt = 1; attempt <= maxAttempts; attempt++) {
                    ct.ThrowIfCancellationRequested();
                    progress?.Report($"Attempt {attempt}/{maxAttempts}: Generating layout...");

                    int rings = rnd.Next(p.MinRings, p.MaxRings + 1);
                    int sectors = rnd.Next(p.MinSectors, p.MaxSectors + 1);
                    int areasCount = rnd.Next(p.MinAreas, p.MaxAreas + 1);

                    var colorPool = Enum.GetValues(typeof(CircleColorType))
                        .Cast<CircleColorType>()
                        .Where(c => c != CircleColorType.None)
                        .OrderBy(x => rnd.Next())
                        .Take(rings)
                        .ToList();

                    var statuses = new SegmentStatus[rings, sectors];
                    var initialColors = new byte[rings, sectors];
                    for (int r = 0; r < rings; r++) {
                        for (int s = 0; s < sectors; s++) {
                            bool blocked = p.AllowBlocked && (rnd.NextDouble() < p.BlockedChance);
                            statuses[r, s] = blocked ? SegmentStatus.Blocked : SegmentStatus.Default;
                            initialColors[r, s] = (byte)colorPool[r];
                        }
                    }

                    var areas = GenerateValidAreas(rings, sectors, areasCount, rnd, p.MinAreaSpan, p.MaxAreaSpan);
                    
                    if (p.AllowFilterColors) {
                        foreach (var area in areas) {
                            if (rnd.NextDouble() < p.FilterColorsChance) {
                                area.SlideAreaStatus = Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors;
                                
                                var allowedColors = new HashSet<CircleColorType>();
                                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                                    allowedColors.Add((CircleColorType)initialColors[r, area.sectorIndex]);
                                }
                                
                                int targetColorCount = rnd.Next(p.MinFilterColors, p.MaxFilterColors + 1);
                                var pool = colorPool.OrderBy(x => rnd.Next()).ToList();
                                foreach (var c in pool) {
                                    if (allowedColors.Count >= targetColorCount) break;
                                    allowedColors.Add(c);
                                }
                                
                                area.Colors = allowedColors.ToList();
                            }
                        }
                    }

                    var state = new LevelState(rings, sectors);
                    for (int r = 0; r < rings; r++)
                        for (int s = 0; s < sectors; s++)
                            state.Colors[r, s] = initialColors[r, s];

                    var solver = new LevelSolver(areas, rings, sectors);
                    for (int r = 0; r < rings; r++)
                        for (int s = 0; s < sectors; s++)
                            if (statuses[r, s] == SegmentStatus.Blocked)
                                solver.SetLockedSegment(r, s, initialColors[r, s]);

                    progress?.Report($"Attempt {attempt}: Scrambling...");
                    var currentLevel = state;
                    int scrambleSteps = Math.Max(targetDifficulty, rnd.Next(targetDifficulty, targetDifficulty + 5));
                    
                    for (int i = 0; i < scrambleSteps; i++) {
                        if (i % 5 == 0) ct.ThrowIfCancellationRequested();
                        var moves = GetAllValidMoves(currentLevel, areas, statuses, solver).ToList();
                        if (moves.Count == 0) break;
                        
                        var move = moves[rnd.Next(moves.Count)];
                        currentLevel = ApplyMove(currentLevel, move, areas, solver);
                    }

                    progress?.Report($"Attempt {attempt}: Calculating difficulty...");
                    int difficulty = solver.Solve(currentLevel, out _);
                    
                    if (difficulty >= targetDifficulty) {
                        progress?.Report("Target difficulty reached!");
                        return new RawLevelData {
                            Rings = rings, Sectors = sectors, Colors = (byte[,])currentLevel.Colors.Clone(),
                            Statuses = statuses, Areas = areas, Difficulty = difficulty
                        };
                    }

                    if (bestLevel == null || (difficulty > 0 && (bestLevel.Difficulty <= 0 || difficulty > bestLevel.Difficulty))) {
                        bestLevel = new RawLevelData {
                            Rings = rings, Sectors = sectors, Colors = (byte[,])currentLevel.Colors.Clone(),
                            Statuses = statuses, Areas = areas, Difficulty = difficulty
                        };
                    }
                }

                progress?.Report("Could not reach target difficulty, returning best found.");
                return bestLevel;
            }, ct);
        }

        private List<SlideAreaConfig> GenerateValidAreas(int rings, int sectors, int count, Random rnd, int minSpan, int maxSpan) {
            var areas = new List<SlideAreaConfig>();
            minSpan = Math.Max(2, Math.Min(minSpan, rings));
            maxSpan = Math.Max(minSpan, Math.Min(maxSpan, rings));

            var uncoveredRings = new HashSet<int>(Enumerable.Range(0, rings));

            // Phase 1: Ensure every ring is covered by at least one slide area
            int safetyLimit = 100;
            while (uncoveredRings.Count > 0 && safetyLimit-- > 0) {
                int targetR = uncoveredRings.ElementAt(rnd.Next(uncoveredRings.Count));
                int attempts = 0;
                while (attempts++ < 100) {
                    int s = rnd.Next(0, sectors);
                    int span = rnd.Next(minSpan, maxSpan + 1);
                    int minStart = Math.Max(0, targetR - span + 1);
                    int maxStart = Math.Min(rings - span, targetR);
                    int startR = rnd.Next(minStart, maxStart + 1);
                    int endR = startR + span - 1;

                    if (!HasConflict(areas, s, startR, endR, sectors)) {
                        var newArea = new SlideAreaConfig {
                            sectorIndex = s, startCircleIndex = startR, endCircleIndex = endR,
                            totalSegments = sectors,
                            SlideAreaStatus = Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.Default,
                            Colors = new List<CircleColorType>()
                        };
                        areas.Add(newArea);
                        for (int r = startR; r <= endR; r++) uncoveredRings.Remove(r);
                        break;
                    }
                }
            }

            // Phase 2: Add more areas to reach the target count if possible
            while (areas.Count < count) {
                int attempts = 0;
                bool areaAdded = false;
                while (attempts++ < 100) {
                    int s = rnd.Next(0, sectors);
                    int span = rnd.Next(minSpan, maxSpan + 1);
                    int startR = rnd.Next(0, rings - span + 1);
                    int endR = startR + span - 1;

                    if (!HasConflict(areas, s, startR, endR, sectors)) {
                        areas.Add(new SlideAreaConfig {
                            sectorIndex = s, startCircleIndex = startR, endCircleIndex = endR,
                            totalSegments = sectors,
                            SlideAreaStatus = Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.Default,
                            Colors = new List<CircleColorType>()
                        });
                        areaAdded = true;
                        break;
                    }
                }
                if (!areaAdded) break;
            }

            return areas;
        }

        private bool HasConflict(List<SlideAreaConfig> areas, int s, int startR, int endR, int sectors) {
            foreach (var existing in areas) {
                // Rule: No identical circle ranges across any sectors
                if (existing.startCircleIndex == startR && existing.endCircleIndex == endR) return true;

                int sectorDiff = Math.Abs(existing.sectorIndex - s);
                sectorDiff = Math.Min(sectorDiff, sectors - sectorDiff);

                // If in the same or adjacent sector, arenas cannot share any rings
                if (sectorDiff <= 1) {
                    if (startR <= existing.endCircleIndex && endR >= existing.startCircleIndex) return true;
                }
            }
            return false;
        }

        private IEnumerable<Move> GetAllValidMoves(LevelState state, List<SlideAreaConfig> areas, SegmentStatus[,] statuses, LevelSolver solver) {
            // Rotation moves
            for (int r = 0; r < state.RingCount; r++) {
                bool hasBlocked = false;
                for(int s=0; s < state.SectorCount; s++) if (statuses[r,s] == SegmentStatus.Blocked) { hasBlocked = true; break; }
                if (hasBlocked) continue;

                for (int offset = 1; offset < state.SectorCount; offset++) {
                    var next = state.Rotate(r, offset);
                    if (!next.Equals(state)) yield return new Move { Type = MoveType.Rotate, Index = r, Offset = offset };
                }
            }

            // Slide moves
            for (int a = 0; a < areas.Count; a++) {
                var area = areas[a];
                bool hasBlocked = false;
                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) if (statuses[r, area.sectorIndex] == SegmentStatus.Blocked) { hasBlocked = true; break; }
                if (hasBlocked) continue;

                int span = area.endCircleIndex - area.startCircleIndex + 1;
                for (int offset = 1; offset < span; offset++) {
                    yield return new Move { Type = MoveType.Slide, Index = a, Offset = offset };
                }
            }
        }

        private LevelState ApplyMove(LevelState state, Move move, List<SlideAreaConfig> areas, LevelSolver solver) {
            if (move.Type == MoveType.Rotate) return state.Rotate(move.Index, move.Offset);
            return solver.ApplyMove(state, move);
        }
    }
}
