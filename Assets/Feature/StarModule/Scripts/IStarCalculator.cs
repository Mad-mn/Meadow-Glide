namespace Feature.StarModule.Scripts {
    public interface IStarCalculator {
        StarRating Calculate(int shortestPath, int averageMoves, int movesUsed);
    }
}
