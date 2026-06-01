using System;
using System.Collections.Generic;
using System.Linq;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class LevelSolver {
        private readonly List<SlideAreaConfig> _areas;
        private readonly byte[,] _lockedSegments; 
        private readonly int _maxIterations;

        public LevelSolver(IReadOnlyList<SlideAreaConfig> areas, int rings, int sectors, int maxIterations = 50000) {
            _areas = new List<SlideAreaConfig>(areas);
            _lockedSegments = new byte[rings, sectors];
            _maxIterations = maxIterations;
        }

        public void SetLockedSegment(int ring, int sector, byte color) {
            _lockedSegments[ring, sector] = color;
        }

        public int Solve(LevelState initialState, out List<Move> solution) {
            solution = null;
            if (initialState.IsSolved()) return 0;

            // A* with Priority Queue simulation
            var openSet = new SortedList<float, List<(LevelState state, List<Move> path)>>();
            var closedSet = new HashSet<LevelState>();

            float initialH = GetHeuristic(initialState);
            openSet.Add(initialH, new List<(LevelState, List<Move>)> { (initialState, new List<Move>()) });

            int iterations = 0;
            while (openSet.Count > 0 && iterations < _maxIterations) {
                iterations++;
                
                float firstKey = openSet.Keys[0];
                var list = openSet.Values[0];
                var (current, path) = list[0];
                
                list.RemoveAt(0);
                if (list.Count == 0) openSet.RemoveAt(0);

                if (current.IsSolved()) {
                    solution = path;
                    return path.Count;
                }

                if (closedSet.Contains(current)) continue;
                closedSet.Add(current);

                foreach (var move in GetAllPossibleMoves(current)) {
                    var nextState = ApplyMove(current, move);
                    if (closedSet.Contains(nextState)) continue;

                    var nextPath = new List<Move>(path) { move };
                    float g = nextPath.Count;
                    float h = GetHeuristic(nextState);
                    float f = g + h;

                    if (!openSet.TryGetValue(f, out var targetList)) {
                        targetList = new List<(LevelState, List<Move>)>();
                        openSet.Add(f, targetList);
                    }
                    targetList.Add((nextState, nextPath));
                }
            }

            return -1;
        }

        private float GetHeuristic(LevelState state) {
            float score = 0;
            for (int r = 0; r < state.RingCount; r++) {
                int[] counts = new int[256];
                for (int s = 0; s < state.SectorCount; s++) counts[state.Colors[r, s]]++;
                
                int max = 0;
                for (int i = 0; i < 256; i++) if (counts[i] > max) max = counts[i];
                
                score += (state.SectorCount - max);
            }
            return score;
        }

        private IEnumerable<Move> GetAllPossibleMoves(LevelState state) {
            // Rotations
            for (int r = 0; r < state.RingCount; r++) {
                // RULE: If any segment in ring is blocked, ring cannot rotate
                bool ringBlocked = false;
                for(int s=0; s < state.SectorCount; s++) {
                    if (_lockedSegments[r, s] != 0) { ringBlocked = true; break; }
                }
                if (ringBlocked) continue;

                // Check every possible offset
                for (int offset = 1; offset < state.SectorCount; offset++) {
                    var next = state.Rotate(r, offset);
                    if (!next.Equals(state)) {
                        yield return new Move { Type = MoveType.Rotate, Index = r, Offset = offset };
                    }
                }
            }

            // Slides
            for (int a = 0; a < _areas.Count; a++) {
                var area = _areas[a];
                
                // RULE: If any segment in area path is blocked, cannot slide
                bool slideBlocked = false;
                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                    if (_lockedSegments[r, area.sectorIndex] != 0) { slideBlocked = true; break; }
                }
                if (slideBlocked) continue;

                int span = area.endCircleIndex - area.startCircleIndex + 1;
                for (int offset = 1; offset < span; offset++) {
                    var next = ApplySlideMove(state, a, offset);
                    if (!next.Equals(state)) {
                        yield return new Move { Type = MoveType.Slide, Index = a, Offset = offset };
                    }
                }
            }
        }

        public LevelState ApplyMove(LevelState state, Move move) {
            if (move.Type == MoveType.Rotate) return state.Rotate(move.Index, move.Offset);
            return ApplySlideMove(state, move.Index, move.Offset);
        }

        private LevelState ApplySlideMove(LevelState state, int areaIndex, int offset) {
            var area = _areas[areaIndex];
            
            if (area.SlideAreaStatus == SlideAreaStatus.FilterColors) {
                int span = area.endCircleIndex - area.startCircleIndex + 1;
                for (int r = 0; r < span; r++) {
                    int currentR = area.startCircleIndex + r;
                    int targetR = area.startCircleIndex + (r + offset) % span;
                    byte color = state.Colors[currentR, area.sectorIndex];
                    if (!area.Colors.Contains((CircleColorType)color)) return state;
                }
            }

            return state.Slide(area.sectorIndex, area.startCircleIndex, area.endCircleIndex, offset);
        }
    }
}
