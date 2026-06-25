using System;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.SaveDataModule.Scripts.SavedData {
    [Serializable]
    public class PlayerProgressData : ISaveData {
        public int Level = 1;
    }

    [Serializable]
    public class PlayerSettingsData : ISaveData {
        public bool SoundsEnabled = true;
        public bool VibrationEnabled = true;
        public Language SelectedLanguage = Language.English;
    }
}