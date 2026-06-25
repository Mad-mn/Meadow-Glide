using System;
using System.Collections.Generic;
using System.Linq;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.StatusModule.Scripts.Segments;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class EmptySegmentScorer {
        public struct ScoredCandidate {
            public int RingIndex;
            public int SectorIndex;
            public float Score;
            public CircleColorType Color;
            public bool InSlideArea;
        }

        private const float AmbiguityMaxScore = 3f;
        private const float FrequencyMaxScore = 2f;
        private const float SlideAreaMaxScore = 2f;
        private const float PatternBreakerPenalty = -1f;
        private const float PatternBreakerBonusIfMulti = 2f;
        private const float PositionMaxScore = 1f;

        public List<ScoredCandidate> ScoreCandidates(
            byte[,] colors,
            SegmentStatus[,] statuses,
            int rings,
            int sectors,
            List<SlideAreaConfig> areas) {

            var candidates = new List<ScoredCandidate>();
            var areaSet = BuildAreaSet(areas, rings, sectors);

            for (int r = 0; r < rings; r++) {
                for (int s = 0; s < sectors; s++) {
                    if (statuses[r, s] != SegmentStatus.Default)
                        continue;

                    float score = 0f;
                    var color = (CircleColorType)colors[r, s];

                    score += ScoreAmbiguity(r, s, color, colors, sectors);
                    score += ScoreFrequency(r, s, color, colors, sectors);
                    score += ScoreSlideArea(r, s, areaSet);
                    score += ScorePatternBreaker(r, s, color, colors, sectors);
                    score += ScorePosition(s, sectors);

                    candidates.Add(new ScoredCandidate {
                        RingIndex = r,
                        SectorIndex = s,
                        Score = score,
                        Color = color,
                        InSlideArea = areaSet.Contains((r, s))
                    });
                }
            }

            return candidates.OrderByDescending(c => c.Score).ToList();
        }

        public List<ScoredCandidate> SelectByScore(
            List<ScoredCandidate> scored,
            int targetCount,
            float minScore,
            Random rnd) {

            var eligible = scored.Where(c => c.Score >= minScore).ToList();

            if (eligible.Count <= targetCount)
                return FilterUniqueColors(eligible);

            var shuffled = eligible.OrderBy(_ => rnd.Next()).ToList();
            return FilterUniqueColors(shuffled.Take(targetCount).ToList());
        }

        public List<ScoredCandidate> SelectHybrid(
            List<ScoredCandidate> scored,
            int targetCount,
            float minScore,
            int topKForSolver,
            Func<byte[,], SegmentStatus[,], int, int, int, int, List<SlideAreaConfig>, ScoredCandidate, float> solverEvaluator,
            byte[,] colors,
            SegmentStatus[,] statuses,
            int rings,
            int sectors,
            List<SlideAreaConfig> areas,
            Random rnd) {

            var eligible = scored.Where(c => c.Score >= minScore).ToList();
            if (eligible.Count == 0)
                return new List<ScoredCandidate>();

            var topK = eligible.Take(Math.Min(topKForSolver, eligible.Count)).ToList();

            for (int i = 0; i < topK.Count; i++) {
                var candidate = topK[i];
                float solverScore = solverEvaluator(colors, statuses, rings, sectors, candidate.RingIndex, candidate.SectorIndex, areas, candidate);
                topK[i] = new ScoredCandidate {
                    RingIndex = candidate.RingIndex,
                    SectorIndex = candidate.SectorIndex,
                    Score = candidate.Score * 0.4f + solverScore * 0.6f,
                    Color = candidate.Color,
                    InSlideArea = candidate.InSlideArea
                };
            }

            var remaining = eligible.Skip(topKForSolver).ToList();
            var all = topK.Concat(remaining).OrderByDescending(c => c.Score).ToList();

            if (all.Count <= targetCount)
                return FilterUniqueColors(all);

            return FilterUniqueColors(all.Take(targetCount).ToList());
        }

        private List<ScoredCandidate> FilterUniqueColors(List<ScoredCandidate> candidates) {
            var usedColors = new HashSet<CircleColorType>();
            var result = new List<ScoredCandidate>();
            foreach (var c in candidates) {
                if (usedColors.Contains(c.Color)) continue;
                usedColors.Add(c.Color);
                result.Add(c);
            }
            return result;
        }

        private float ScoreAmbiguity(int ring, int sector, CircleColorType color, byte[,] colors, int sectors) {
            int sameColorInStripe = 0;
            for (int s = 0; s < sectors; s++) {
                if (s != sector && (CircleColorType)colors[ring, s] == color)
                    sameColorInStripe++;
            }

            if (sameColorInStripe == 0) return 0f;
            if (sameColorInStripe == 1) return AmbiguityMaxScore * 0.5f;
            if (sameColorInStripe >= 2) return AmbiguityMaxScore;
            return AmbiguityMaxScore * 0.3f;
        }

        private float ScoreFrequency(int ring, int sector, CircleColorType color, byte[,] colors, int sectors) {
            int colorCount = 0;
            for (int s = 0; s < sectors; s++) {
                if ((CircleColorType)colors[ring, s] == color)
                    colorCount++;
            }

            float ratio = (float)colorCount / sectors;

            if (ratio > 0.5f)
                return FrequencyMaxScore;

            if (ratio > 0.3f)
                return FrequencyMaxScore * 0.7f;

            return FrequencyMaxScore * 0.3f;
        }

        private float ScoreSlideArea(int ring, int sector, HashSet<(int, int)> areaSet) {
            return areaSet.Contains((ring, sector)) ? SlideAreaMaxScore : 0f;
        }

        private float ScorePatternBreaker(int ring, int sector, CircleColorType color, byte[,] colors, int sectors) {
            int colorCount = 0;
            for (int s = 0; s < sectors; s++) {
                if ((CircleColorType)colors[ring, s] == color)
                    colorCount++;
            }

            bool isSoleNonDominant = colorCount == 1 && sectors > 2;

            if (isSoleNonDominant)
                return PatternBreakerPenalty;

            int dominantCount = 0;
            CircleColorType dominantColor = CircleColorType.None;
            for (int s = 0; s < sectors; s++) {
                var c = (CircleColorType)colors[ring, s];
                int count = 0;
                for (int ss = 0; ss < sectors; ss++) {
                    if ((CircleColorType)colors[ring, ss] == c) count++;
                }
                if (count > dominantCount) {
                    dominantCount = count;
                    dominantColor = c;
                }
            }

            if (color == dominantColor && colorCount >= 3) {
                return PatternBreakerBonusIfMulti;
            }

            return 0f;
        }

        private float ScorePosition(int sector, int sectors) {
            bool isEdge = sector == 0 || sector == sectors - 1;
            bool isCenter = sector == sectors / 2;

            if (isEdge)
                return 0f;

            if (isCenter)
                return PositionMaxScore;

            return PositionMaxScore * 0.5f;
        }

        private HashSet<(int, int)> BuildAreaSet(List<SlideAreaConfig> areas, int rings, int sectors) {
            var set = new HashSet<(int, int)>();
            foreach (var area in areas) {
                for (int r = area.startCircleIndex; r <= area.endCircleIndex; r++) {
                    set.Add((r, area.sectorIndex));
                }
            }
            return set;
        }
    }
}
