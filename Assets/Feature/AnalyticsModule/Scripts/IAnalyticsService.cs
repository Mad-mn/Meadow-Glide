namespace Feature.AnalyticsModule.Scripts {
    public interface IAnalyticsService {
        void LevelStarted(int levelId);
        void LevelCompleted(int levelId, int attempts, int movesUsed);
        void UndoMoveUsed(int levelId);
        void ExtraMovesPurchased(int levelId);
        void DailyChallengeStarted(int levelId);
        void DailyChallengeCompleted(int levelId, bool isPerfect);
        void PerfectMapLevelStarted(int levelId);
    }
}
