using System;
using System.Collections.Generic;
using System.Linq;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class LevelAnalyzer {
        private readonly List<SlideAreaConfig> _areas;
        private readonly int _ringCount;
        private readonly int _sectorCount;

        public LevelAnalyzer(IReadOnlyList<SlideAreaConfig> areas, int ringCount, int sectorCount) {
            _areas = new List<SlideAreaConfig>(areas);
            _ringCount = ringCount;
            _sectorCount = sectorCount;
        }

        public LevelMetrics Analyze(LevelState state) {
            return new LevelMetrics {
                AvgStripeComplexity = CalculateStripeComplexity(state),
                ColorDispersion = CalculateColorDispersion(state),
                DependencyScore = CalculateDependencyScore(state),
                ArenaInteraction = CalculateArenaInteraction(state),
                MisleadingRatio = 0f,
                CandidateDiversity = 0f
            };
        }

        public LevelMetrics AnalyzeWithSolver(LevelState state, LevelSolver solver, List<Move> solution) {
            var metrics = Analyze(state);

            if (solution != null && solution.Count > 0) {
                var misleading = CalculateMisleadingMoves(state, solver, solution);
                metrics.MisleadingRatio = misleading.AvgMisleading;
                metrics.CandidateDiversity = misleading.AvgDiversity;
            }

            return metrics;
        }

        private float CalculateStripeComplexity(LevelState state) {
            float total = 0;
            for (int r = 0; r < state.RingCount; r++) {
                var colors = new HashSet<byte>();
                for (int s = 0; s < state.SectorCount; s++)
                    colors.Add(state.Colors[r, s]);
                total += (colors.Count - 1);
            }
            return total / state.RingCount;
        }

        private float CalculateColorDispersion(LevelState state) {
            var colorPositions = new Dictionary<byte, List<(int r, int s)>>();
            for (int r = 0; r < state.RingCount; r++) {
                for (int s = 0; s < state.SectorCount; s++) {
                    byte c = state.Colors[r, s];
                    if (!colorPositions.ContainsKey(c))
                        colorPositions[c] = new List<(int, int)>();
                    colorPositions[c].Add((r, s));
                }
            }

            float totalDispersion = 0;
            int colorCount = 0;

            foreach (var kvp in colorPositions) {
                var positions = kvp.Value;
                if (positions.Count <= 1) continue;

                float meanR = (float)positions.Average(p => p.r);
                float meanS = (float)positions.Average(p => p.s);

                float variance = 0;
                foreach (var (r, s) in positions) {
                    float dr = r - meanR;
                    float ds = s - meanS;
                    variance += dr * dr + ds * ds;
                }
                variance /= positions.Count;

                totalDispersion += variance;
                colorCount++;
            }

            return colorCount > 0 ? totalDispersion / colorCount : 0;
        }

        private float CalculateDependencyScore(LevelState state) {
            if (_areas.Count == 0) return 0;

            float totalDependency = 0;
            foreach (var area in _areas) {
                var colors = new HashSet<byte>();
                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++)
                    colors.Add(state.Colors[r, area.sectorIndex]);
                totalDependency += (colors.Count - 1);
            }
            return totalDependency / _areas.Count;
        }

        private float CalculateArenaInteraction(LevelState state) {
            if (_areas.Count <= 1) return 0;

            float totalInteraction = 0;
            int pairCount = 0;

            for (int i = 0; i < _areas.Count; i++) {
                for (int j = i + 1; j < _areas.Count; j++) {
                    var a1 = _areas[i];
                    var a2 = _areas[j];

                    if (a1.sectorIndex == a2.sectorIndex) {
                        int overlapStart = Math.Max(a1.startCircleIndex, a2.startCircleIndex);
                        int overlapEnd = Math.Min(a1.endCircleIndex, a2.endCircleIndex);
                        if (overlapStart <= overlapEnd) {
                            int overlapLength = overlapEnd - overlapStart + 1;
                            int minSpan = Math.Min(
                                a1.endCircleIndex - a1.startCircleIndex + 1,
                                a2.endCircleIndex - a2.startCircleIndex + 1);
                            totalInteraction += (float)overlapLength / minSpan;
                        }
                    } else {
                        bool shareRing = false;
                        for (int r = a1.startCircleIndex; r <= a1.endCircleIndex && !shareRing; r++) {
                            if (r >= a2.startCircleIndex && r <= a2.endCircleIndex)
                                shareRing = true;
                        }
                        if (shareRing)
                            totalInteraction += 0.5f;
                    }
                    pairCount++;
                }
            }

            return pairCount > 0 ? totalInteraction / pairCount : 0;
        }

        private MisleadingResult CalculateMisleadingMoves(LevelState state, LevelSolver solver, List<Move> solution) {
            int totalMisleading = 0;
            int totalViable = 0;
            int steps = 0;

            var currentState = state;
            foreach (var move in solution) {
                var allMoves = solver.GetAllPossibleMoves(currentState).ToList();
                if (allMoves.Count == 0) break;

                float currentH = solver.GetHeuristic(currentState);
                int productive = 0;
                int viable = 0;

                foreach (var m in allMoves) {
                    var nextState = solver.ApplyMove(currentState, m);
                    float nextH = solver.GetHeuristic(nextState);

                    if (nextH < currentH) productive++;
                    if (nextH <= currentH + 1) viable++;
                }

                totalMisleading += (allMoves.Count - productive);
                totalViable += viable;
                steps++;

                currentState = solver.ApplyMove(currentState, move);
            }

            return new MisleadingResult {
                AvgMisleading = steps > 0 ? (float)totalMisleading / (steps * GetTotalMoves(state)) : 0,
                AvgDiversity = steps > 0 ? (float)totalViable / steps : 0
            };
        }

        private int GetTotalMoves(LevelState state) {
            int count = state.RingCount * (state.SectorCount - 1);
            foreach (var area in _areas) {
                int span = area.endCircleIndex - area.startCircleIndex + 1;
                count += (span - 1);
            }
            return count;
        }

        public float CalculateDifficulty(LevelMetrics metrics, int pathLength) {
            float maxStripeComplexity = _sectorCount - 1;
            float maxDispersion = _ringCount * _sectorCount * 0.25f;
            float maxDependency = _sectorCount - 1;
            float maxInteraction = 1.0f;

            float multiplier = 1.0f
                + 0.20f * Math.Min(metrics.AvgStripeComplexity / maxStripeComplexity, 1.0f)
                + 0.15f * Math.Min(metrics.ColorDispersion / maxDispersion, 1.0f)
                + 0.25f * Math.Min(metrics.DependencyScore / maxDependency, 1.0f)
                + 0.20f * Math.Min(metrics.ArenaInteraction / maxInteraction, 1.0f)
                + 0.10f * metrics.MisleadingRatio
                + 0.10f * Math.Max(0f, 1.0f - metrics.CandidateDiversity / GetTotalMoves(new LevelState(_ringCount, _sectorCount)));

            return pathLength * multiplier;
        }

        private struct MisleadingResult {
            public float AvgMisleading;
            public float AvgDiversity;
        }
    }

    public struct LevelMetrics {
        public float AvgStripeComplexity;
        public float ColorDispersion;
        public float DependencyScore;
        public float ArenaInteraction;
        public float MisleadingRatio;
        public float CandidateDiversity;
    }
}
