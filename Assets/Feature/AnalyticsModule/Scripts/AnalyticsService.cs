namespace Feature.AnalyticsModule.Scripts {
    public class AnalyticsService : IAnalyticsService {
        public void LevelStarted(int levelId) { }

        public void LevelCompleted(int levelId, int attempts, int movesUsed) { }

        public void UndoMoveUsed(int levelId) { }

        public void ExtraMovesPurchased(int levelId) { }

        public void DailyChallengeStarted(int levelId) { }

        public void DailyChallengeCompleted(int levelId, bool isPerfect) { }

        public void PerfectMapLevelStarted(int levelId) { }
    }
}
