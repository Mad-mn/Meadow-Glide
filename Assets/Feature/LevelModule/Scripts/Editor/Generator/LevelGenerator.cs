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
                                // Always include the solution colors to ensure it starts solvable
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
                    // Increase scramble steps proportional to target difficulty
                    int scrambleSteps = Math.Max(targetDifficulty, rnd.Next(targetDifficulty, targetDifficulty * 2));
                    
                    for (int i = 0; i < scrambleSteps; i++) {
                        if (i % 10 == 0) ct.ThrowIfCancellationRequested();
                        var moves = GetAllValidMoves(currentLevel, areas, statuses).ToList();
                        if (moves.Count == 0) break;
                        
                        var move = moves[rnd.Next(moves.Count)];
                        currentLevel = ApplyMove(currentLevel, move, areas);
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

                    if (bestLevel == null || difficulty > bestLevel.Difficulty) {
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
            
            // Ensure span is within valid bounds
            minSpan = Math.Max(2, Math.Min(minSpan, rings));
            maxSpan = Math.Max(minSpan, Math.Min(maxSpan, rings));

            for (int i = 0; i < count; i++) {
                int attempts = 0;
                while (attempts++ < 100) {
                    int s = rnd.Next(0, sectors);
                    int span = rnd.Next(minSpan, maxSpan + 1);
                    int startR = rnd.Next(0, rings - span + 1);
                    int endR = startR + span - 1;

                    bool conflict = false;
                    foreach (var existing in areas) {
                        int sectorDiff = Math.Abs(existing.sectorIndex - s);
                        // Wrap around distance
                        sectorDiff = Math.Min(sectorDiff, sectors - sectorDiff);

                        if (sectorDiff == 0) {
                            // Same sector - check overlap
                            if (startR <= existing.endCircleIndex && endR >= existing.startCircleIndex) {
                                conflict = true;
                                break;
                            }
                        } else if (sectorDiff == 1) {
                            // Adjacent sectors - check if more than 1 ring is shared
                            int overlapStart = Math.Max(startR, existing.startCircleIndex);
                            int overlapEnd = Math.Min(endR, existing.endCircleIndex);
                            int sharedRings = Math.Max(0, overlapEnd - overlapStart + 1);

                            if (sharedRings > 1) {
                                conflict = true;
                                break;
                            }
                        }
                    }

                    if (!conflict) {
                        areas.Add(new SlideAreaConfig {
                            sectorIndex = s,
                            startCircleIndex = startR,
                            endCircleIndex = endR,
                            totalSegments = sectors,
                            SlideAreaStatus = Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.Default,
                            Colors = new List<CircleColorType>()
                        });
                        break;
                    }
                }
            }
            return areas;
        }

        private IEnumerable<Move> GetAllValidMoves(LevelState state, List<SlideAreaConfig> areas, SegmentStatus[,] statuses) {
            for (int r = 0; r < state.RingCount; r++) {
                yield return new Move { Type = MoveType.Rotate, Index = r, Direction = 1 };
                yield return new Move { Type = MoveType.Rotate, Index = r, Direction = -1 };
            }
            for (int a = 0; a < areas.Count; a++) {
                var area = areas[a];
                
                // Check blocked segments
                bool blocked = false;
                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                    if (statuses[r, area.sectorIndex] == SegmentStatus.Blocked) {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) continue;

                // Check FilterColors
                if (area.SlideAreaStatus == Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors) {
                    bool allowed = true;
                    for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                        if (!area.Colors.Contains((CircleColorType)state.Colors[r, area.sectorIndex])) {
                            allowed = false;
                            break;
                        }
                    }
                    if (!allowed) continue;
                }

                yield return new Move { Type = MoveType.Slide, Index = a, Direction = 1 };
                yield return new Move { Type = MoveType.Slide, Index = a, Direction = -1 };
            }
        }

        private LevelState ApplyMove(LevelState state, Move move, List<SlideAreaConfig> areas) {
            if (move.Type == MoveType.Rotate) return state.Rotate(move.Index, move.Direction);
            var area = areas[move.Index];
            
            // Double check FilterColors here just in case, though GetAllValidMoves should handle it
            if (area.SlideAreaStatus == Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors) {
                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                    if (!area.Colors.Contains((CircleColorType)state.Colors[r, area.sectorIndex])) return state;
                }
            }
            
            return state.Slide(area.sectorIndex, area.startCircleIndex, area.endCircleIndex, move.Direction);
        }
    }
}
