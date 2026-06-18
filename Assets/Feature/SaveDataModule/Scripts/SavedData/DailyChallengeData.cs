using System;

namespace Feature.SaveDataModule.Scripts.SavedData {
    [Serializable]
    public class DailyChallengeData : ISaveData {
        public string LastCompletedDate = "";
        public int TodayStarsEarned;
        public int ClaimedStarsThreshold;
    }
}
