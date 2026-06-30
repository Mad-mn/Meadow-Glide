using System.Collections.Generic;
using UnityEngine;

namespace Feature.AnalyticsModule.Scripts {
    public static class AnalyticsEvents {
        public const string UndoMoveUsed = "UndoMoveUsed";
        public const string ExtraMovesPurchased = "ExtraMovesPurchased";
        public const string DailyChallengeStarted = "DailyChallengeStarted";
        public const string DailyChallengeCompleted = "DailyChallengeCompleted";
        public const string PerfectMapLevelStarted = "PerfectMapLevelStarted";
    }

    public static class AnalyticsParams {
        public const string LevelId = "level_id";
        public const string IsPerfect = "is_perfect";
    }

    public class AnalyticsService : IAnalyticsService {
        public void LevelStarted(int levelId) {
            Debug.Log($"[Analytics] LevelStarted: level_id={levelId}");
            TinySauce.OnGameStarted(levelId);
        }

        public void LevelCompleted(int levelId, int attempts, int movesUsed) {
            Debug.Log($"[Analytics] LevelCompleted: level_id={levelId}, attempts={attempts}, moves_used={movesUsed}");
            TinySauce.OnGameFinished(true, movesUsed, levelId);
        }

        public void UndoMoveUsed(int levelId) {
            SendCustomEvent(AnalyticsEvents.UndoMoveUsed, levelId);
        }

        public void ExtraMovesPurchased(int levelId) {
            SendCustomEvent(AnalyticsEvents.ExtraMovesPurchased, levelId);
        }

        public void DailyChallengeStarted(int levelId) {
            SendCustomEvent(AnalyticsEvents.DailyChallengeStarted, levelId);
        }

        public void DailyChallengeCompleted(int levelId, bool isPerfect) {
            var props = new Dictionary<string, object> {
                { AnalyticsParams.LevelId, levelId },
                { AnalyticsParams.IsPerfect, isPerfect }
            };
            Debug.Log($"[Analytics] DailyChallengeCompleted: level_id={levelId}, is_perfect={isPerfect}");
            TinySauce.TrackCustomEvent(AnalyticsEvents.DailyChallengeCompleted, props);
        }

        public void PerfectMapLevelStarted(int levelId) {
            SendCustomEvent(AnalyticsEvents.PerfectMapLevelStarted, levelId);
        }

        private static void SendCustomEvent(string eventName, int levelId) {
            var props = new Dictionary<string, object> {
                { AnalyticsParams.LevelId, levelId }
            };
            Debug.Log($"[Analytics] {eventName}: level_id={levelId}");
            TinySauce.TrackCustomEvent(eventName, props);
        }
    }
}
