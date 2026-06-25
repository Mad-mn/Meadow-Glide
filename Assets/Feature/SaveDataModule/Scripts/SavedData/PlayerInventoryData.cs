using System;
using System.Collections.Generic;
using Feature.PlayerInventoryModule.Scripts;

namespace Feature.SaveDataModule.Scripts.SavedData
{
    [Serializable]
    public class PlayerInventoryData : ISaveData
    {
        public Dictionary<ResourceType, int> Balances = new Dictionary<ResourceType, int>();
    }
}
