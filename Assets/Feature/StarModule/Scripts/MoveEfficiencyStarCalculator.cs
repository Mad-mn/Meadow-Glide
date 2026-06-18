using UnityEngine;

namespace Feature.StarModule.Scripts {
    public class MoveEfficiencyStarCalculator : IStarCalculator {
        private readonly float _threeStarThreshold;
        private readonly float _twoStarThreshold;

        public MoveEfficiencyStarCalculator(float threeStarThreshold = 0.5f, float twoStarThreshold = 0.25f) {
            _threeStarThreshold = threeStarThreshold;
            _twoStarThreshold = twoStarThreshold;
        }

        public StarRating Calculate(int maxMoves, int movesUsed) {
            if (maxMoves <= 0)
                return StarRating.One;

            int movesLeft = maxMoves - movesUsed;
            float ratio = (float)movesLeft / maxMoves;

            if (ratio >= _threeStarThreshold)
                return StarRating.Three;

            if (ratio >= _twoStarThreshold)
                return StarRating.Two;

            return StarRating.One;
        }
    }
}
