namespace Feature.StarModule.Scripts {
    public interface IStarCalculator {
        StarRating Calculate(int maxMoves, int movesUsed);
    }
}
