using System;

namespace Feature.SaveDataModule.Scripts.SavedData {
    [Serializable]
    public class PlayerProgressData : ISaveData {
        public int Level = 1;
    }
}