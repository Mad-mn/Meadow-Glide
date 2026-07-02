using System.Collections.Generic;
using ByteBrewSDK;

namespace Feature.AnalyticsModule.Scripts {
    public static class AnalyticsEvents {
        public const string LevelStarted = "LevelStarted";
        public const string LevelCompleted = "LevelCompleted";
        public const string UndoMoveUsed = "UndoMoveUsed";
        public const string ExtraMovesPurchased = "ExtraMovesPurchased";
        public const string DailyChallengeStarted = "DailyChallengeStarted";
        public const string DailyChallengeCompleted = "DailyChallengeCompleted";
        public const string PerfectMapLevelStarted = "PerfectMapLevelStarted";
    }

    public static class AnalyticsParams {
        public const string LevelId = "level_id";
        public const string Attempts = "attempts";
        public const string MovesUsed = "moves_used";
        public const string IsPerfect = "is_perfect";
    }

    public class AnalyticsService : IAnalyticsService {
        public void LevelStarted(int levelId) {
            var parameters = new Dictionary<string, string> {
                { AnalyticsParams.LevelId, levelId.ToString() }
            };
            ByteBrew.NewCustomEvent(AnalyticsEvents.LevelStarted, parameters);
        }

        public void LevelCompleted(int levelId, int attempts, int movesUsed) {
            var parameters = new Dictionary<string, string> {
                { AnalyticsParams.LevelId, levelId.ToString() },
                { AnalyticsParams.Attempts, attempts.ToString() },
                { AnalyticsParams.MovesUsed, movesUsed.ToString() }
            };
            ByteBrew.NewCustomEvent(AnalyticsEvents.LevelCompleted, parameters);
        }

        public void UndoMoveUsed(int levelId) {
            SendByteBrewEvent(AnalyticsEvents.UndoMoveUsed, levelId);
        }

        public void ExtraMovesPurchased(int levelId) {
            SendByteBrewEvent(AnalyticsEvents.ExtraMovesPurchased, levelId);
        }

        public void DailyChallengeStarted(int levelId) {
            SendByteBrewEvent(AnalyticsEvents.DailyChallengeStarted, levelId);
        }

        public void DailyChallengeCompleted(int levelId, bool isPerfect) {
            var parameters = new Dictionary<string, string> {
                { AnalyticsParams.LevelId, levelId.ToString() },
                { AnalyticsParams.IsPerfect, isPerfect.ToString() }
            };
            ByteBrew.NewCustomEvent(AnalyticsEvents.DailyChallengeCompleted, parameters);
        }

        public void PerfectMapLevelStarted(int levelId) {
            SendByteBrewEvent(AnalyticsEvents.PerfectMapLevelStarted, levelId);
        }

        private static void SendByteBrewEvent(string eventName, int levelId) {
            var parameters = new Dictionary<string, string> {
                { AnalyticsParams.LevelId, levelId.ToString() }
            };
            ByteBrew.NewCustomEvent(eventName, parameters);
        }
    }
}
