using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class DifficultyCalculator {
        private readonly LevelSolver _solver;
        private readonly List<SlideAreaConfig> _areas;
        private readonly int _rings;

        public DifficultyCalculator(LevelSolver solver, List<SlideAreaConfig> areas, int rings) {
            _solver = solver;
            _areas = areas;
            _rings = rings;
        }

        public int Calculate(LevelState state, int maxMoves,
            out int pathLength, out int averageMoves, out float avgConfusion, out float avgPlanningDepth) {

            avgPlanningDepth = CalculatePlanningDepth();

            pathLength = _solver.Solve(state, out var solution);

            if (pathLength <= 0) {
                avgConfusion = 0f;
                averageMoves = 0;
                return pathLength;
            }

            avgConfusion = CalculateAvgConfusion(state, solution);

            float gapFactor = 0.3f + avgConfusion * 0.4f;
            float gap = (maxMoves - pathLength) * gapFactor;
            averageMoves = Mathf.RoundToInt(pathLength + gap);
            averageMoves = Mathf.Clamp(averageMoves, pathLength + 1, maxMoves - 1);

            float multiplier = 1f + avgConfusion + avgPlanningDepth;
            return Math.Max(1, Mathf.RoundToInt(pathLength * multiplier));
        }

        private float CalculateAvgConfusion(LevelState initialState, List<Move> solution) {
            if (solution == null || solution.Count == 0) return 0f;

            float totalConfusion = 0f;
            var currentState = initialState;

            foreach (var move in solution) {
                float currentH = _solver.GetHeuristic(currentState);
                var allMoves = _solver.GetAllPossibleMoves(currentState).ToList();

                if (allMoves.Count == 0) break;

                int productiveMoves = 0;
                foreach (var candidateMove in allMoves) {
                    var nextState = _solver.ApplyMove(currentState, candidateMove);
                    float nextH = _solver.GetHeuristic(nextState);
                    if (nextH < currentH) {
                        productiveMoves++;
                    }
                }

                int nonProductiveMoves = allMoves.Count - productiveMoves;
                float confusion = allMoves.Count > 0 ? (float)nonProductiveMoves / allMoves.Count : 0f;
                totalConfusion += confusion;

                currentState = _solver.ApplyMove(currentState, move);
            }

            return totalConfusion / solution.Count;
        }

        private float CalculatePlanningDepth() {
            if (_areas == null || _areas.Count == 0 || _rings <= 1) return 0f;

            float maxLog = (float)Math.Log(_rings, 2);
            if (maxLog <= 0f) return 0f;

            float totalDepth = 0f;
            foreach (var area in _areas) {
                int span = area.endCircleIndex - area.startCircleIndex + 1;
                float depth = (float)Math.Log(span, 2) / maxLog;
                totalDepth += depth;
            }

            return totalDepth / _areas.Count;
        }
    }
}
