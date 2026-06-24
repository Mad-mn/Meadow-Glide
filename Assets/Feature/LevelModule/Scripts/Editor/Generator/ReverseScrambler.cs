using System;
using System.Collections.Generic;
using System.Linq;
using Feature.ColorServiceModule.Scripts;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class ReverseScrambler {
        private readonly List<SlideAreaConfig> _areas;
        private readonly int _ringCount;
        private readonly int _sectorCount;

        public ReverseScrambler(IReadOnlyList<SlideAreaConfig> areas, int ringCount, int sectorCount) {
            _areas = new List<SlideAreaConfig>(areas);
            _ringCount = ringCount;
            _sectorCount = sectorCount;
        }

        public ScrambleResult Scramble(LevelState solvedState, int targetDepth, Random rnd, int maxAttempts = 10) {
            for (int attempt = 0; attempt < maxAttempts; attempt++) {
                var result = TryScramble(solvedState, targetDepth, rnd);
                if (result.MoveCount >= targetDepth)
                    return result;
            }

            return TryScramble(solvedState, targetDepth, rnd);
        }

        private ScrambleResult TryScramble(LevelState solvedState, int targetDepth, Random rnd) {
            var current = solvedState;
            var moves = new List<Move>();
            var recentMoves = new Queue<Move>(3);

            for (int step = 0; step < targetDepth + 10; step++) {
                var candidates = GetScrambleCandidates(current, recentMoves, moves.Count, targetDepth);
                if (candidates.Count == 0) break;

                var scored = ScoreCandidates(candidates, current, recentMoves, rnd);
                var best = scored[0].Move;

                current = ApplyMove(current, best);
                moves.Add(best);

                recentMoves.Enqueue(best);
                if (recentMoves.Count > 3) recentMoves.Dequeue();

                if (moves.Count >= targetDepth) break;
            }

            return new ScrambleResult {
                State = current,
                Moves = moves,
                MoveCount = moves.Count
            };
        }

        private List<Move> GetScrambleCandidates(LevelState state, Queue<Move> recentMoves, int currentDepth, int targetDepth) {
            var candidates = new List<Move>();
            var lastMove = recentMoves.Count > 0 ? recentMoves.Last() : default;

            for (int r = 0; r < state.RingCount; r++) {
                for (int offset = 1; offset < state.SectorCount; offset++) {
                    var move = new Move { Type = MoveType.Rotate, Index = r, Offset = offset };

                    if (IsUndo(move, lastMove)) continue;

                    var next = state.Rotate(r, offset);
                    if (!next.Equals(state))
                        candidates.Add(move);
                }
            }

            for (int a = 0; a < _areas.Count; a++) {
                var area = _areas[a];
                int span = area.endCircleIndex - area.startCircleIndex + 1;
                for (int offset = 1; offset < span; offset++) {
                    var move = new Move { Type = MoveType.Slide, Index = a, Offset = offset };

                    if (IsUndo(move, lastMove)) continue;

                    var next = ApplySlideMove(state, a, offset);
                    if (!next.Equals(state))
                        candidates.Add(move);
                }
            }

            return candidates;
        }

        private List<ScoredMove> ScoreCandidates(List<Move> candidates, LevelState state, Queue<Move> recentMoves, Random rnd) {
            var scored = new List<ScoredMove>(candidates.Count);
            var recentSet = new HashSet<Move>(recentMoves);

            foreach (var move in candidates) {
                float score = 0;

                var nextState = ApplyMove(state, move);

                int conflictBefore = CountConflicts(state);
                int conflictAfter = CountConflicts(nextState);
                score += (conflictAfter - conflictBefore) * 3.0f;

                if (recentSet.Contains(move))
                    score -= 5.0f;

                score += GetDiversityBonus(move, recentMoves) * 1.0f;

                score += (float)(rnd.NextDouble() * 1.0 - 0.5);

                scored.Add(new ScoredMove { Move = move, Score = score });
            }

            scored.Sort((a, b) => b.Score.CompareTo(a.Score));
            return scored;
        }

        private int CountConflicts(LevelState state) {
            int conflicts = 0;
            for (int r = 0; r < state.RingCount; r++) {
                byte dominant = GetDominantColor(state, r);
                for (int s = 0; s < state.SectorCount; s++) {
                    if (state.Colors[r, s] != dominant)
                        conflicts++;
                }
            }
            return conflicts;
        }

        private byte GetDominantColor(LevelState state, int ring) {
            var counts = new int[256];
            for (int s = 0; s < state.SectorCount; s++)
                counts[state.Colors[ring, s]]++;

            byte best = 0;
            int bestCount = 0;
            for (int c = 1; c < 256; c++) {
                if (counts[c] > bestCount) {
                    bestCount = counts[c];
                    best = (byte)c;
                }
            }
            return best;
        }

        private float GetDiversityBonus(Move move, Queue<Move> recentMoves) {
            float bonus = 0;

            bool usedRotate = false;
            bool usedSlide = false;
            foreach (var m in recentMoves) {
                if (m.Type == MoveType.Rotate) usedRotate = true;
                if (m.Type == MoveType.Slide) usedSlide = true;
            }

            if (move.Type == MoveType.Rotate && !usedRotate) bonus += 0.5f;
            if (move.Type == MoveType.Slide && !usedSlide) bonus += 0.5f;

            var usedAreas = new HashSet<int>();
            var usedRings = new HashSet<int>();
            foreach (var m in recentMoves) {
                if (m.Type == MoveType.Rotate) usedRings.Add(m.Index);
                if (m.Type == MoveType.Slide) usedAreas.Add(m.Index);
            }

            if (move.Type == MoveType.Rotate && !usedRings.Contains(move.Index)) bonus += 0.3f;
            if (move.Type == MoveType.Slide && !usedAreas.Contains(move.Index)) bonus += 0.3f;

            return bonus;
        }

        private bool IsUndo(Move current, Move previous) {
            if (previous.Type != current.Type || previous.Index != current.Index) return false;

            if (current.Type == MoveType.Rotate) {
                return (current.Offset + previous.Offset) % _sectorCount == 0;
            }

            var area = _areas[current.Index];
            int span = area.endCircleIndex - area.startCircleIndex + 1;
            return (current.Offset + previous.Offset) % span == 0;
        }

        private LevelState ApplyMove(LevelState state, Move move) {
            if (move.Type == MoveType.Rotate) return state.Rotate(move.Index, move.Offset);
            return ApplySlideMove(state, move.Index, move.Offset);
        }

        private LevelState ApplySlideMove(LevelState state, int areaIndex, int offset) {
            var area = _areas[areaIndex];
            return state.Slide(area.sectorIndex, area.startCircleIndex, area.endCircleIndex, offset);
        }

        public struct ScrambleResult {
            public LevelState State;
            public List<Move> Moves;
            public int MoveCount;
        }

        private struct ScoredMove {
            public Move Move;
            public float Score;
        }
    }
}
