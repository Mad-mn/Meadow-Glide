using System;
using System.Collections.Generic;
using Feature.LocalizationModule.Scripts.Data;
using Feature.MoveEfficiencyModule.Scripts;

namespace Feature.SaveDataModule.Scripts.SavedData {
    [Serializable]
    public class LevelCompletionData : ISaveData {
        public MoveEfficiencyResult Status;
        public int Attempts;
        public int MovesUsed;
    }

    [Serializable]
    public class PlayerProgressData : ISaveData {
        public int Level = 1;
        public Dictionary<int, LevelCompletionData> CompletedLevels = new Dictionary<int, LevelCompletionData>();
        public HashSet<int> ClaimedPerfectMapRewards = new HashSet<int>();
        public HashSet<int> CompletedTutorials = new HashSet<int>();
    }

    [Serializable]
    public class PlayerSettingsData : ISaveData {
        public bool SoundsEnabled = true;
        public bool MusicEnabled = true;
        public bool VibrationEnabled = true;
        public bool NotificationsEnabled = true;
        public Language SelectedLanguage = Language.English;
    }
}