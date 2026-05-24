using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.LevelModule.Scripts {
    [CreateAssetMenu(fileName = "LevelConfigProvider", menuName = "Configs/LevelConfigs/LevelConfigProvider")]
    public class LevelConfigProvider : ScriptableObject, ISerializationCallbackReceiver {
        [SerializeField] private List<LevelData> _levelDatas;

        private Dictionary<int, LevelData> _levelDataDictionary;

        public IReadOnlyDictionary<int, LevelData> LevelDatas {
            get {
                if (_levelDataDictionary == null) {
                    ToDictionary();
                }

                return _levelDataDictionary;
            }
        }

        [ContextMenu("Generate Level")]
        public void ToDictionary() {
            _levelDataDictionary = new Dictionary<int, LevelData>();

            if (_levelDatas == null) return;

            foreach (LevelData levelData in _levelDatas) {
                if (levelData == null) continue;

                if (!_levelDataDictionary.ContainsKey(levelData.LevelID)) {
                    _levelDataDictionary.Add(levelData.LevelID, levelData);
                }
            }
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() {
            _levelDataDictionary = null;
        }
    }

    [Serializable]
    public class LevelData {
        public int LevelID;
        public LevelConfig LevelConfig;
    }
}