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
            public bool AllowBlocked;
            public float BlockedChance;
            public bool AllowFilterColors;
            public float FilterColorsChance;
        }

        public class RawLevelData {
            public int Rings;
            public int Sectors;
            public byte[,] Colors;
            public SegmentStatus[,] Statuses;
            public List<SlideAreaConfig> Areas;
            public int Difficulty;
        }

        public async Task<RawLevelData> GenerateAsync(GenerationParams p, int targetDifficulty) {
            return await Task.Run(() => {
                var rnd = new Random();
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

                var areas = GenerateValidAreas(rings, sectors, areasCount, rnd);
                
                // Assign FilterColors
                if (p.AllowFilterColors) {
                    foreach (var area in areas) {
                        if (rnd.NextDouble() < p.FilterColorsChance) {
                            area.SlideAreaStatus = Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors;
                            
                            // To ensure it's movable from solved state, include the solved colors
                            var allowedColors = new HashSet<CircleColorType>();
                            for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                                allowedColors.Add((CircleColorType)initialColors[r, area.sectorIndex]);
                            }
                            
                            // Maybe add 1-2 more random colors from the pool
                            int extraColors = rnd.Next(0, 2);
                            for (int i = 0; i < extraColors; i++) {
                                allowedColors.Add(colorPool[rnd.Next(colorPool.Count)]);
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

                var currentLevel = state;
                int scrambleSteps = rnd.Next(5, 15);
                for (int i = 0; i < scrambleSteps; i++) {
                    var moves = GetAllValidMoves(currentLevel, areas, statuses).ToList();
                    if (moves.Count == 0) break;
                    
                    var move = moves[rnd.Next(moves.Count)];
                    var next = ApplyMove(currentLevel, move, areas);
                    if (!next.Equals(currentLevel)) {
                        currentLevel = next;
                    } else {
                        // If move was invalid due to FilterColors (even though GetAllValidMoves should have checked)
                        i--; // retry
                    }
                }

                int difficulty = solver.Solve(currentLevel, out _);
                if (difficulty < 0) return null;

                return new RawLevelData {
                    Rings = rings,
                    Sectors = sectors,
                    Colors = (byte[,])currentLevel.Colors.Clone(),
                    Statuses = statuses,
                    Areas = areas,
                    Difficulty = difficulty
                };
            });
        }

        private List<SlideAreaConfig> GenerateValidAreas(int rings, int sectors, int count, Random rnd) {
            var areas = new List<SlideAreaConfig>();
            var used = new HashSet<int>();
            for (int i = 0; i < count; i++) {
                int attempts = 0;
                while (attempts++ < 50) {
                    int s = rnd.Next(0, sectors);
                    bool conflict = false;
                    for (int offset = -1; offset <= 1; offset++) {
                        if (used.Contains((s + offset + sectors) % sectors)) conflict = true;
                    }
                    if (!conflict) {
                        areas.Add(new SlideAreaConfig {
                            sectorIndex = s,
                            startCircleIndex = 0,
                            endCircleIndex = rings - 1,
                            totalSegments = rings,
                            SlideAreaStatus = Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.Default,
                            Colors = new List<CircleColorType>()
                        });
                        used.Add(s);
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
