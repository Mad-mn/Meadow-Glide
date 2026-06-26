using Feature.PlayerInventoryModule.Scripts;

namespace Feature.PerfectMapViewModule.Scripts {
    public enum LevelPerfectState {
        NotCompleted,
        CompletedNotPerfect,
        PerfectNotClaimed,
        PerfectClaimed
    }

    public struct LevelInfoPerfectData {
        public int LevelNumber;
        public int BestMoves;
        public int ShortestSolution;
        public LevelPerfectState State;
        public ResourceType RewardType;
        public int RewardAmount;
    }
}
