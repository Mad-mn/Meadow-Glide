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
            public bool AllowEmptySegments;
            public float EmptyChance;
            public int MinEmptySegments, MaxEmptySegments;
            public bool UseIntelligentEmpty;
            public float EmptyMinScore;
            public int EmptyTopKForSolver;
            public int MaxAttempts;
            public int MaxIterations;
            public int Seed;
            public bool UseFixedSeed;
            public int TargetSolutionLength;
            public int MaxCandidatesPerLevel;
        }

        public struct ValidationResult {
            public bool IsValid;
            public string Reason;

            public static ValidationResult Success() => new ValidationResult { IsValid = true };
            public static ValidationResult Fail(string reason) => new ValidationResult { IsValid = false, Reason = reason };
        }

        public class RawLevelData {
            public int Rings;
            public int Sectors;
            public byte[,] Colors;
            public SegmentStatus[,] Statuses;
            public List<SlideAreaConfig> Areas;
            public int Difficulty;
            public int AverageMoves;
            public List<Move> SolutionPath;
            public float AvgConfusion;
            public float AvgPlanningDepth;
            public int Seed;
        }

        public async Task<RawLevelData> GenerateAsync(GenerationParams p, int targetDifficulty, System.Threading.CancellationToken ct, IProgress<string> progress = null) {
            return await Task.Run(() => {
                int activeSeed = p.UseFixedSeed ? p.Seed : unchecked((int)Guid.NewGuid().GetHashCode());
                var rnd = new Random(activeSeed);
                int maxAttempts = Math.Max(1, p.MaxAttempts);
                RawLevelData bestLevel = null;

                progress?.Report($"Seed: {activeSeed}");

                for (int attempt = 1; attempt <= maxAttempts; attempt++) {
                    ct.ThrowIfCancellationRequested();
                    progress?.Report($"Attempt {attempt}/{maxAttempts}: Generating layout...");

                    int rings = rnd.Next(p.MinRings, p.MaxRings + 1);
                    int sectors = rnd.Next(p.MinSectors, p.MaxSectors + 1);
                    int areasCount = rnd.Next(p.MinAreas, p.MaxAreas + 1);

                    var colorPool = Enum.GetValues(typeof(CircleColorType))
                        .Cast<CircleColorType>()
                        .Where(c => c != CircleColorType.None)
                        .OrderBy(_ => rnd.Next())
                        .ToList();

                    var statuses = new SegmentStatus[rings, sectors];
                    var initialColors = new byte[rings, sectors];
                    var blockedColors = new HashSet<byte>();
                    for (int r = 0; r < rings; r++) {
                        for (int s = 0; s < sectors; s++) {
                            initialColors[r, s] = (byte)colorPool[r % colorPool.Count];
                            bool blocked = p.AllowBlocked && rnd.NextDouble() < p.BlockedChance
                                && !blockedColors.Contains(initialColors[r, s]);
                            statuses[r, s] = blocked ? SegmentStatus.Blocked : SegmentStatus.Default;
                            if (blocked) blockedColors.Add(initialColors[r, s]);
                        }
                    }

                    var areas = GenerateValidAreas(rings, sectors, areasCount, rnd, p.MinAreaSpan, p.MaxAreaSpan);

                    if (p.AllowFilterColors) {
                        foreach (var area in areas) {
                            if (rnd.NextDouble() < p.FilterColorsChance) {
                                area.SlideAreaStatus = Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors;

                                var allowedColors = new HashSet<CircleColorType>();
                                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++)
                                    allowedColors.Add((CircleColorType)initialColors[r, area.sectorIndex]);

                                int targetColorCount = rnd.Next(p.MinFilterColors, p.MaxFilterColors + 1);
                                var pool = colorPool.OrderBy(_ => rnd.Next()).ToList();
                                foreach (var c in pool) {
                                    if (allowedColors.Count >= targetColorCount) break;
                                    allowedColors.Add(c);
                                }

                                area.Colors = allowedColors.ToList();
                            }
                        }
                    }

                    var layoutValidation = ValidateLayout(rings, areas);
                    if (!layoutValidation.IsValid) {
                        LogValidationFailure(attempt, layoutValidation.Reason);
                        continue;
                    }

                    bool hasBlocked = false;
                    for (int r = 0; r < rings && !hasBlocked; r++)
                        for (int s = 0; s < sectors && !hasBlocked; s++)
                            if (statuses[r, s] == SegmentStatus.Blocked) hasBlocked = true;

                    var state = new LevelState(rings, sectors, hasBlocked);
                    for (int r = 0; r < rings; r++)
                        for (int s = 0; s < sectors; s++) {
                            state.Colors[r, s] = initialColors[r, s];
                            if (state.Blocked != null && statuses[r, s] == SegmentStatus.Blocked)
                                state.Blocked[r, s] = initialColors[r, s];
                        }

                    progress?.Report($"Attempt {attempt}: Reverse scrambling...");

                    int targetSolutionLength = p.TargetSolutionLength > 0 ? p.TargetSolutionLength : targetDifficulty;
                    var scrambler = new ReverseScrambler(areas, rings, sectors);
                    var scrambleResult = scrambler.Scramble(state, targetSolutionLength, rnd);

                    var validation = ValidateGeneratedLevel(scrambleResult.State, rings, areas);
                    if (!validation.IsValid) {
                        LogValidationFailure(attempt, validation.Reason);
                        continue;
                    }

                    progress?.Report($"Attempt {attempt}: Solving...");

                    var solver = new LevelSolver(areas, rings, sectors, Math.Max(1, p.MaxIterations));
                    int pathLength = solver.Solve(scrambleResult.State, out var solution);

                    if (pathLength < 0) {
                        LogValidationFailure(attempt, "Level unsolvable after reverse scramble.");
                        continue;
                    }

                    progress?.Report($"Attempt {attempt}: Analyzing difficulty...");

                    var analyzer = new LevelAnalyzer(areas, rings, sectors);
                    var metrics = analyzer.AnalyzeWithSolver(scrambleResult.State, solver, solution);
                    float calculatedDifficulty = analyzer.CalculateDifficulty(metrics, pathLength);
                    int difficulty = Mathf.RoundToInt(calculatedDifficulty);

                    int maxMovesForLevel = Mathf.RoundToInt(targetDifficulty * 2.5f);
                    var calculator = new DifficultyCalculator(solver, areas, rings);
                    int legacyDifficulty = calculator.Calculate(scrambleResult.State, maxMovesForLevel,
                        out _, out int averageMoves, out float confusion, out float planningDepth);

                    var pruned = PruneLevel(scrambleResult.State, areas, solution);

                    if (p.AllowEmptySegments) {
                        ApplyEmptySegments(pruned.Statuses, pruned.Colors, pruned.Rings, pruned.Sectors, pruned.Areas, rnd, p);
                    }

                    var candidate = new RawLevelData {
                        Rings = pruned.Rings,
                        Sectors = pruned.Sectors,
                        Colors = pruned.Colors,
                        Statuses = pruned.Statuses,
                        Areas = pruned.Areas,
                        Difficulty = difficulty,
                        AverageMoves = averageMoves,
                        SolutionPath = solution,
                        AvgConfusion = confusion,
                        AvgPlanningDepth = planningDepth,
                        Seed = activeSeed
                    };

                    if (difficulty >= targetDifficulty) {
                        progress?.Report($"Target difficulty reached! (Seed: {activeSeed})");
                        return candidate;
                    }

                    if (bestLevel == null || (difficulty > 0 && (bestLevel.Difficulty <= 0 || difficulty > bestLevel.Difficulty)))
                        bestLevel = candidate;
                }

                progress?.Report(bestLevel != null
                    ? $"Could not reach target difficulty, returning best found. (Seed: {activeSeed})"
                    : $"Generation failed after {maxAttempts} attempts. (Seed: {activeSeed})");
                return bestLevel;
            }, ct);
        }

        public static ValidationResult ValidateGeneratedLevel(LevelState state, int ringCount, List<SlideAreaConfig> areas) {
            if (state.IsSolved())
                return ValidationResult.Fail("Level is already solved after scramble.");

            return ValidateRingConnectivity(ringCount, areas);
        }

        public static ValidationResult ValidateLayout(int ringCount, List<SlideAreaConfig> areas) {
            if (areas == null || areas.Count == 0)
                return ValidationResult.Fail("Level has no slide areas.");

            return ValidateRingConnectivity(ringCount, areas);
        }

        public static ValidationResult ValidateRingConnectivity(int ringCount, List<SlideAreaConfig> areas) {
            if (ringCount <= 1)
                return ValidationResult.Success();

            var parent = Enumerable.Range(0, ringCount).ToArray();
            var rank = new int[ringCount];

            int Find(int x) {
                while (parent[x] != x) {
                    parent[x] = parent[parent[x]];
                    x = parent[x];
                }
                return x;
            }

            void Union(int a, int b) {
                int rootA = Find(a);
                int rootB = Find(b);
                if (rootA == rootB) return;
                if (rank[rootA] < rank[rootB])
                    parent[rootA] = rootB;
                else if (rank[rootA] > rank[rootB])
                    parent[rootB] = rootA;
                else {
                    parent[rootB] = rootA;
                    rank[rootA]++;
                }
            }

            var coveredRings = new HashSet<int>();
            foreach (var area in areas) {
                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                    coveredRings.Add(r);
                    if (r > area.startCircleIndex)
                        Union(r, r - 1);
                }
            }

            var uncovered = Enumerable.Range(0, ringCount).Where(r => !coveredRings.Contains(r)).ToList();
            if (uncovered.Count > 0)
                return ValidationResult.Fail($"Rings not covered by any slide area: [{string.Join(", ", uncovered)}].");

            int root = Find(0);
            var unreachable = Enumerable.Range(1, ringCount - 1).Where(r => Find(r) != root).ToList();
            if (unreachable.Count > 0)
                return ValidationResult.Fail($"Rings not connected via slide areas. Unreachable from ring 0: [{string.Join(", ", unreachable)}].");

            return ValidationResult.Success();
        }

        private static void LogValidationFailure(int attempt, string reason) {
            Debug.LogWarning($"[LevelGenerator] Attempt {attempt}: Validation failed — {reason}");
        }

        private List<SlideAreaConfig> GenerateValidAreas(int rings, int sectors, int count, Random rnd, int minSpan, int maxSpan) {
            var areas = new List<SlideAreaConfig>();
            minSpan = Math.Max(2, Math.Min(minSpan, rings));
            maxSpan = Math.Max(minSpan, Math.Min(maxSpan, rings));

            var uncoveredRings = new HashSet<int>(Enumerable.Range(0, rings));

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
                        areas.Add(new SlideAreaConfig {
                            sectorIndex = s, startCircleIndex = startR, endCircleIndex = endR,
                            totalSegments = sectors,
                            SlideAreaStatus = Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.Default,
                            Colors = new List<CircleColorType>()
                        });
                        for (int r = startR; r <= endR; r++) uncoveredRings.Remove(r);
                        break;
                    }
                }
            }

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
                if (existing.startCircleIndex == startR && existing.endCircleIndex == endR) return true;

                int sectorDiff = Math.Abs(existing.sectorIndex - s);
                sectorDiff = Math.Min(sectorDiff, sectors - sectorDiff);

                if (sectorDiff <= 1) {
                    if (startR <= existing.endCircleIndex && endR >= existing.startCircleIndex) return true;
                }
            }
            return false;
        }

        private void ApplyEmptySegments(SegmentStatus[,] statuses, byte[,] colors, int rings, int sectors,
            List<SlideAreaConfig> areas, Random rnd, GenerationParams p) {
            int totalSegments = rings * sectors;
            int targetEmpty = rnd.Next(p.MinEmptySegments, Math.Min(p.MaxEmptySegments + 1, totalSegments));

            if (targetEmpty <= 0) return;

            var scorer = new EmptySegmentScorer();
            var scored = scorer.ScoreCandidates(colors, statuses, rings, sectors, areas);

            List<EmptySegmentScorer.ScoredCandidate> selected;

            if (p.UseIntelligentEmpty && scored.Count > 0) {
                selected = scorer.SelectHybrid(
                    scored, targetEmpty, p.EmptyMinScore,
                    Math.Min(p.EmptyTopKForSolver, scored.Count),
                    EvaluateCandidateWithSolver,
                    colors, statuses, rings, sectors, areas, rnd);
            } else {
                selected = scorer.SelectByScore(scored, targetEmpty, p.EmptyMinScore, rnd);
            }

            foreach (var c in selected) {
                statuses[c.RingIndex, c.SectorIndex] = SegmentStatus.Empty;
            }
        }

        private float EvaluateCandidateWithSolver(
            byte[,] colors, SegmentStatus[,] statuses,
            int rings, int sectors,
            int testRing, int testSector,
            List<SlideAreaConfig> areas,
            EmptySegmentScorer.ScoredCandidate candidate) {

            var testStatuses = (SegmentStatus[,])statuses.Clone();
            testStatuses[testRing, testSector] = SegmentStatus.Empty;

            bool hasBlocked = false;
            for (int r = 0; r < rings && !hasBlocked; r++)
                for (int s = 0; s < sectors && !hasBlocked; s++)
                    if (testStatuses[r, s] == SegmentStatus.Blocked) hasBlocked = true;

            var state = new LevelState(rings, sectors, hasBlocked);
            for (int r = 0; r < rings; r++)
                for (int s = 0; s < sectors; s++) {
                    state.Colors[r, s] = colors[r, s];
                    if (state.Blocked != null && testStatuses[r, s] == SegmentStatus.Blocked)
                        state.Blocked[r, s] = colors[r, s];
                }

            var solver = new LevelSolver(areas, rings, sectors, 5000);

            int movesToSolve = solver.Solve(state, out _);

            float score = 0f;

            if (movesToSolve > 15) score += 3f;
            else if (movesToSolve > 8) score += 2f;
            else if (movesToSolve > 3) score += 1f;

            int sameColorCount = 0;
            for (int s = 0; s < sectors; s++) {
                if (s != testSector && (CircleColorType)colors[testRing, s] == candidate.Color)
                    sameColorCount++;
            }

            if (sameColorCount >= 2) score += 1f;
            else if (sameColorCount == 0) score -= 1f;

            return Mathf.Clamp(score, 0f, 10f);
        }

        public struct PrunedLevelData {
            public int Rings;
            public int Sectors;
            public byte[,] Colors;
            public SegmentStatus[,] Statuses;
            public List<SlideAreaConfig> Areas;
        }

        public static PrunedLevelData PruneLevel(LevelState state, List<SlideAreaConfig> areas, List<Move> solution) {
            int rings = state.RingCount;
            int sectors = state.SectorCount;

            if (solution == null || solution.Count == 0) {
                var defaultStatuses = new SegmentStatus[rings, sectors];
                if (state.Blocked != null) {
                    for (int r = 0; r < rings; r++)
                        for (int s = 0; s < sectors; s++)
                            defaultStatuses[r, s] = state.Blocked[r, s] != 0 ? SegmentStatus.Blocked : SegmentStatus.Default;
                }
                return new PrunedLevelData {
                    Rings = rings,
                    Sectors = sectors,
                    Colors = (byte[,])state.Colors.Clone(),
                    Statuses = defaultStatuses,
                    Areas = new List<SlideAreaConfig>(areas)
                };
            }

            var usedRings = new HashSet<int>();
            var usedAreaIndices = new HashSet<int>();

            foreach (var move in solution) {
                if (move.Type == MoveType.Rotate) {
                    usedRings.Add(move.Index);
                } else {
                    usedAreaIndices.Add(move.Index);
                    var area = areas[move.Index];
                    for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                        usedRings.Add(r);
                    }
                }
            }

            var removedRings = new List<int>();
            var keptRings = new List<int>();
            for (int r = 0; r < rings; r++) {
                if (usedRings.Contains(r)) {
                    keptRings.Add(r);
                } else {
                    removedRings.Add(r);
                }
            }

            if (keptRings.Count == rings && usedAreaIndices.Count == areas.Count) {
                var noChangeStatuses = new SegmentStatus[rings, sectors];
                if (state.Blocked != null) {
                    for (int r = 0; r < rings; r++)
                        for (int s = 0; s < sectors; s++)
                            noChangeStatuses[r, s] = state.Blocked[r, s] != 0 ? SegmentStatus.Blocked : SegmentStatus.Default;
                }
                return new PrunedLevelData {
                    Rings = rings,
                    Sectors = sectors,
                    Colors = (byte[,])state.Colors.Clone(),
                    Statuses = noChangeStatuses,
                    Areas = new List<SlideAreaConfig>(areas)
                };
            }

            var ringRemap = new int[rings];
            for (int i = 0; i < rings; i++) ringRemap[i] = -1;
            for (int newIdx = 0; newIdx < keptRings.Count; newIdx++) {
                ringRemap[keptRings[newIdx]] = newIdx;
            }

            int newRingCount = keptRings.Count;
            var newColors = new byte[newRingCount, sectors];
            var newStatuses = new SegmentStatus[newRingCount, sectors];

            for (int newR = 0; newR < newRingCount; newR++) {
                int oldR = keptRings[newR];
                for (int s = 0; s < sectors; s++) {
                    newColors[newR, s] = state.Colors[oldR, s];
                    newStatuses[newR, s] = (state.Blocked != null && state.Blocked[oldR, s] != 0)
                        ? SegmentStatus.Blocked
                        : SegmentStatus.Default;
                }
            }

            var newAreas = new List<SlideAreaConfig>();
            for (int i = 0; i < areas.Count; i++) {
                if (!usedAreaIndices.Contains(i)) continue;

                var old = areas[i];
                newAreas.Add(new SlideAreaConfig {
                    sectorIndex = old.sectorIndex,
                    startCircleIndex = ringRemap[old.startCircleIndex],
                    endCircleIndex = ringRemap[old.endCircleIndex],
                    totalSegments = old.totalSegments,
                    SlideAreaStatus = old.SlideAreaStatus,
                    Colors = old.Colors != null ? new List<CircleColorType>(old.Colors) : new List<CircleColorType>()
                });
            }

            return new PrunedLevelData {
                Rings = newRingCount,
                Sectors = sectors,
                Colors = newColors,
                Statuses = newStatuses,
                Areas = newAreas
            };
        }
    }
}
