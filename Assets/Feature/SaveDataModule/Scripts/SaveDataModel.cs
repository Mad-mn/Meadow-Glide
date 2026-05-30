using System.Collections.Generic;

namespace Feature.SaveDataModule.Scripts
{
    public class SaveDataModel : ISaveDataModel
    {
        private readonly Dictionary<SaveDataType, ISaveData> _cachedData = new Dictionary<SaveDataType, ISaveData>();

        public T Get<T>(SaveDataType type) where T : ISaveData, new()
        {
            if (_cachedData.TryGetValue(type, out var data))
            {
                return (T)data;
            }

            // If not found in model, we return a new instance, but we don't store it here
            // because the Service is responsible for populating the model from disk/defaults.
            return new T();
        }

        public ISaveData GetRaw(SaveDataType type)
        {
            _cachedData.TryGetValue(type, out var data);
            return data;
        }

        public void Set(SaveDataType type, ISaveData data)
        {
            _cachedData[type] = data;
        }
    }
}