using System;
using System.Collections.Generic;
using System.Linq;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class LevelSolver {
        private readonly List<SlideAreaConfig> _areas;
        private readonly int _maxIterations;
        private readonly int _ringCount;
        private readonly int _sectorCount;
        private ZobristTable _zobristTable;
        private int[] _heuristicBuffer;

        public LevelSolver(IReadOnlyList<SlideAreaConfig> areas, int rings, int sectors, int maxIterations = 200000) {
            _areas = new List<SlideAreaConfig>(areas);
            _maxIterations = maxIterations;
            _ringCount = rings;
            _sectorCount = sectors;
            _zobristTable = new ZobristTable(rings, sectors, 256);
            _heuristicBuffer = new int[256];
        }

        public bool IsSlideAreaBlocked(LevelState state, int areaIndex) {
            if (state.Blocked == null) return false;

            var area = _areas[areaIndex];
            bool isFilterColors = area.SlideAreaStatus == SlideAreaStatus.FilterColors;

            for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                if (state.Blocked[r, area.sectorIndex] != 0)
                    return true;

                if (isFilterColors) {
                    var color = (CircleColorType)state.Colors[r, area.sectorIndex];
                    if (area.Colors == null || !area.Colors.Contains(color))
                        return true;
                }
            }

            return false;
        }

        public int Solve(LevelState initialState, out List<Move> solution) {
            solution = null;
            if (initialState.IsSolved()) return 0;

            var initState = initialState;
            initState.ComputeZobristHash(_zobristTable);

            var openSet = new BinaryHeap<ExpandNode>();
            var nodes = new List<ExpandNode>();
            var closedSet = new HashSet<ulong>();
            var bestG = new Dictionary<ulong, int>();

            float initialH = GetHeuristic(initState);
            var startNode = new ExpandNode {
                ParentIndex = -1,
                Move = default,
                G = 0,
                F = initialH,
                ZobristHash = initState.ZobristHash,
                State = initState
            };
            nodes.Add(startNode);
            openSet.Push(startNode);

            int iterations = 0;
            while (openSet.Count > 0 && iterations < _maxIterations) {
                iterations++;
                var current = openSet.Pop();

                if (closedSet.Contains(current.ZobristHash)) continue;
                closedSet.Add(current.ZobristHash);

                var currentState = current.State;
                if (currentState.IsSolved()) {
                    solution = ReconstructPath(nodes, current);
                    return solution.Count;
                }

                foreach (var move in GetAllPossibleMoves(currentState)) {
                    var nextState = ApplyMove(currentState, move);
                    nextState.ComputeZobristHash(_zobristTable);

                    if (closedSet.Contains(nextState.ZobristHash)) continue;

                    int nextG = current.G + 1;
                    if (bestG.TryGetValue(nextState.ZobristHash, out int existingG) && nextG >= existingG)
                        continue;
                    bestG[nextState.ZobristHash] = nextG;

                    float h = GetHeuristic(nextState);
                    var nextNode = new ExpandNode {
                        ParentIndex = current.NodeIndex,
                        Move = move,
                        G = nextG,
                        F = nextG + h,
                        ZobristHash = nextState.ZobristHash,
                        State = nextState
                    };
                    nextNode.NodeIndex = nodes.Count;
                    nodes.Add(nextNode);
                    openSet.Push(nextNode);
                }
            }

            return -1;
        }

        public float GetHeuristic(LevelState state) {
            float score = 0;
            for (int r = 0; r < state.RingCount; r++) {
                Array.Clear(_heuristicBuffer, 0, _heuristicBuffer.Length);
                for (int s = 0; s < state.SectorCount; s++)
                    _heuristicBuffer[state.Colors[r, s]]++;

                int max = 0;
                for (int i = 0; i < _heuristicBuffer.Length; i++)
                    if (_heuristicBuffer[i] > max) max = _heuristicBuffer[i];

                score += (state.SectorCount - max);
            }
            return score;
        }

        public IEnumerable<Move> GetAllPossibleMoves(LevelState state) {
            for (int r = 0; r < state.RingCount; r++) {
                for (int offset = 1; offset < state.SectorCount; offset++) {
                    var next = state.Rotate(r, offset);
                    if (!next.Equals(state)) {
                        yield return new Move { Type = MoveType.Rotate, Index = r, Offset = offset };
                    }
                }
            }

            for (int a = 0; a < _areas.Count; a++) {
                if (IsSlideAreaBlocked(state, a)) continue;

                var area = _areas[a];
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
            if (IsSlideAreaBlocked(state, areaIndex))
                return state;

            var area = _areas[areaIndex];
            return state.Slide(area.sectorIndex, area.startCircleIndex, area.endCircleIndex, offset);
        }

        private List<Move> ReconstructPath(List<ExpandNode> nodes, ExpandNode goal) {
            var path = new List<Move>();
            var current = goal;
            while (current.ParentIndex >= 0) {
                path.Add(current.Move);
                current = nodes[current.ParentIndex];
            }
            path.Reverse();
            return path;
        }

        public int SolveAll(LevelState initialState, int maxDepth, out List<int> solutionLengths) {
            solutionLengths = new List<int>();
            if (initialState.IsSolved()) {
                solutionLengths.Add(0);
                return 0;
            }

            var initState = initialState;
            initState.ComputeZobristHash(_zobristTable);

            var openSet = new BinaryHeap<ExpandNode>();
            var nodes = new List<ExpandNode>();
            var closedSet = new HashSet<ulong>();

            float initialH = GetHeuristic(initState);
            var startNode = new ExpandNode {
                ParentIndex = -1,
                Move = default,
                G = 0,
                F = initialH,
                ZobristHash = initState.ZobristHash,
                State = initState
            };
            nodes.Add(startNode);
            openSet.Push(startNode);

            int iterations = 0;
            int shortest = -1;
            while (openSet.Count > 0 && iterations < _maxIterations) {
                iterations++;
                var current = openSet.Pop();

                if (closedSet.Contains(current.ZobristHash)) continue;
                closedSet.Add(current.ZobristHash);

                var currentState = current.State;
                if (currentState.IsSolved()) {
                    if (shortest < 0) shortest = current.G;
                    solutionLengths.Add(current.G);
                    continue;
                }

                if (current.G >= maxDepth) continue;

                foreach (var move in GetAllPossibleMoves(currentState)) {
                    var nextState = ApplyMove(currentState, move);
                    nextState.ComputeZobristHash(_zobristTable);

                    if (closedSet.Contains(nextState.ZobristHash)) continue;

                    int nextG = current.G + 1;
                    if (nextG > maxDepth) continue;

                    float h = GetHeuristic(nextState);
                    var nextNode = new ExpandNode {
                        ParentIndex = current.NodeIndex,
                        Move = move,
                        G = nextG,
                        F = nextG + h,
                        ZobristHash = nextState.ZobristHash,
                        State = nextState
                    };
                    nextNode.NodeIndex = nodes.Count;
                    nodes.Add(nextNode);
                    openSet.Push(nextNode);
                }
            }

            solutionLengths.Sort();
            return shortest;
        }

        private struct ExpandNode : IComparable<ExpandNode> {
            public int NodeIndex;
            public int ParentIndex;
            public Move Move;
            public int G;
            public float F;
            public ulong ZobristHash;
            public LevelState State;

            public int CompareTo(ExpandNode other) {
                int cmp = F.CompareTo(other.F);
                if (cmp != 0) return cmp;
                return G.CompareTo(other.G);
            }
        }
    }
}
