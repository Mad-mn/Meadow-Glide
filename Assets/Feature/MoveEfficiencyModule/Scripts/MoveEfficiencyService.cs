namespace Feature.MoveEfficiencyModule.Scripts {
    public class MoveEfficiencyService : IMoveEfficiencyService {
        public int MinMoves { get; private set; }
        public MoveEfficiencyResult CurrentResult { get; private set; }

        public void SetMinMoves(int minMoves) {
            MinMoves = minMoves;
        }

        public MoveEfficiencyResult Evaluate(int movesUsed) {
            if (movesUsed <= MinMoves)
                CurrentResult = MoveEfficiencyResult.PerfectClear;
            else
                CurrentResult = MoveEfficiencyResult.Completed;

            return CurrentResult;
        }

        public void Reset() {
            MinMoves = 0;
            CurrentResult = MoveEfficiencyResult.None;
        }
    }
}
