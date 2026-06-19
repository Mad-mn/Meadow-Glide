namespace Feature.StarModule.Scripts {
    public class MoveEfficiencyStarCalculator : IStarCalculator {
        public StarRating Calculate(int shortestPath, int averageMoves, int movesUsed) {
            if (movesUsed <= shortestPath)
                return StarRating.Three;

            if (movesUsed <= averageMoves)
                return StarRating.Two;

            return StarRating.One;
        }
    }
}
