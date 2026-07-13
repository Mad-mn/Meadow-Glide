using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using UnityEngine;
using System;
using Feature.NotificationModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;

namespace Feature.SaveDataModule.Scripts
{
    public class SaveDataService : ISaveDataService
    {
        private readonly ISaveDataModel _model;

        public SaveDataService(ISaveDataModel model)
        {
            _model = model;
        }

        private string GetPath(SaveDataType type)
        {
            return Path.Combine(Application.persistentDataPath, $"{type.ToString().ToLower()}_save.json");
        }

        public void LoadAll()
        {
            _model.Set(SaveDataType.PlayerProgress, LoadFromDisk<PlayerProgressData>(SaveDataType.PlayerProgress));
            _model.Set(SaveDataType.Settings, LoadFromDisk<PlayerSettingsData>(SaveDataType.Settings));
            _model.Set(SaveDataType.PlayerInventory, LoadFromDisk<PlayerInventoryData>(SaveDataType.PlayerInventory));
            _model.Set(SaveDataType.DailyChallenge, LoadFromDisk<DailyChallengeData>(SaveDataType.DailyChallenge));
            _model.Set(SaveDataType.Notifications, LoadFromDisk<NotificationSaveData>(SaveDataType.Notifications));
            Debug.Log("All save data loaded into Model.");
        }

        public void Save(SaveDataType type)
        {
            var data = _model.GetRaw(type);
            if (data != null)
            {
                SaveToDisk(type, data);
            }
        }

        public void SaveAll()
        {
            foreach (SaveDataType type in Enum.GetValues(typeof(SaveDataType)))
            {
                var data = _model.GetRaw(type);
                if (data != null)
                {
                    SaveToDisk(type, data);
                }
            }
            Debug.Log("All save data models saved to disk.");
        }

        public void Clear(SaveDataType type)
        {
            _model.Set(type, null);

            string path = GetPath(type);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public void ClearAll()
        {
            foreach (SaveDataType type in Enum.GetValues(typeof(SaveDataType)))
            {
                Clear(type);
            }
        }

        public bool HasSaveData(SaveDataType type)
        {
            return File.Exists(GetPath(type));
        }

        private T LoadFromDisk<T>(SaveDataType type) where T : ISaveData, new()
        {
            string jsonPath = GetPath(type);

            if (File.Exists(jsonPath))
            {
                try
                {
                    string json = File.ReadAllText(jsonPath);
                    var data = JsonConvert.DeserializeObject<T>(json);
                    Debug.Log($"Successfully loaded {type} from {jsonPath}.");
                    return data;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load {type} from {jsonPath}. Using default parameters. Error: {e.Message}");
                    return new T();
                }
            }

            Debug.Log($"Save file for {type} not found. Creating new data with default parameters.");
            return new T();
        }

        private void SaveToDisk(SaveDataType type, ISaveData data)
        {
            string path = GetPath(type);
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save {type} to {path}: {e.Message}");
            }
        }
    }
}
