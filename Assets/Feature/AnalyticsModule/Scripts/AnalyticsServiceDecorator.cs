using System.Collections.Generic;

namespace Feature.AnalyticsModule.Scripts {
    public class AnalyticsServiceDecorator : IAnalyticsService {
        private readonly AnalyticsService _analyticsService;

        public AnalyticsServiceDecorator(AnalyticsService analyticsService) {
            _analyticsService = analyticsService;
        }

        public void LevelStarted(int levelId) {
            _analyticsService.LevelStarted(levelId);
            TinySauce.OnGameStarted(levelId);
        }

        public void LevelCompleted(int levelId, int attempts, int movesUsed) {
            _analyticsService.LevelCompleted(levelId, attempts, movesUsed);
            TinySauce.OnGameFinished(true, movesUsed, levelId);
        }

        public void UndoMoveUsed(int levelId) {
            _analyticsService.UndoMoveUsed(levelId);
            SendTinySauceEvent(AnalyticsEvents.UndoMoveUsed, levelId);
        }

        public void ExtraMovesPurchased(int levelId) {
            _analyticsService.ExtraMovesPurchased(levelId);
            SendTinySauceEvent(AnalyticsEvents.ExtraMovesPurchased, levelId);
        }

        public void DailyChallengeStarted(int levelId) {
            _analyticsService.DailyChallengeStarted(levelId);
            SendTinySauceEvent(AnalyticsEvents.DailyChallengeStarted, levelId);
        }

        public void DailyChallengeCompleted(int levelId, bool isPerfect) {
            _analyticsService.DailyChallengeCompleted(levelId, isPerfect);
            var props = new Dictionary<string, object> {
                { AnalyticsParams.LevelId, levelId },
                { AnalyticsParams.IsPerfect, isPerfect }
            };
            TinySauce.TrackCustomEvent(AnalyticsEvents.DailyChallengeCompleted, props);
        }

        public void PerfectMapLevelStarted(int levelId) {
            _analyticsService.PerfectMapLevelStarted(levelId);
            SendTinySauceEvent(AnalyticsEvents.PerfectMapLevelStarted, levelId);
        }

        private static void SendTinySauceEvent(string eventName, int levelId) {
            var props = new Dictionary<string, object> {
                { AnalyticsParams.LevelId, levelId }
            };
            TinySauce.TrackCustomEvent(eventName, props);
        }
    }
}
