namespace Feature.MoveEfficiencyModule.Scripts {
    public interface IMoveEfficiencyService {
        int MinMoves { get; }
        MoveEfficiencyResult CurrentResult { get; }
        void SetMinMoves(int minMoves);
        MoveEfficiencyResult Evaluate(int movesUsed);
        void Reset();
    }
}
