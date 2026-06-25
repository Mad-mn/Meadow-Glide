using System;
using System.Collections.Generic;
using Feature.LocalizationModule.Scripts.Data;
using Feature.MoveEfficiencyModule.Scripts;

namespace Feature.SaveDataModule.Scripts.SavedData {
    [Serializable]
    public class LevelCompletionData : ISaveData {
        public MoveEfficiencyResult Status;
        public int Attempts;
    }

    [Serializable]
    public class PlayerProgressData : ISaveData {
        public int Level = 1;
        public Dictionary<int, LevelCompletionData> CompletedLevels = new Dictionary<int, LevelCompletionData>();
    }

    [Serializable]
    public class PlayerSettingsData : ISaveData {
        public bool SoundsEnabled = true;
        public bool VibrationEnabled = true;
        public Language SelectedLanguage = Language.English;
    }
}