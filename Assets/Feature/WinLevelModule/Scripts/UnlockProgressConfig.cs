using System;
using System.Collections.Generic;
using Feature.LocalizationModule.Scripts.Data;
using UnityEngine;

namespace Feature.WinLevelModule.Scripts {
    [CreateAssetMenu(fileName = "UnlockProgressConfig", menuName = "Configs/WinLevel/UnlockProgressConfig")]
    public class UnlockProgressConfig : ScriptableObject {
        [SerializeField] private List<UnlockProgressData> _entries;

        public IReadOnlyList<UnlockProgressData> Entries => _entries;

        public UnlockProgressData GetEntryForLevel(int playerLevelBeforeGame) {
            for (int i = 0; i < _entries.Count; i++) {
                if (playerLevelBeforeGame < _entries[i].UnlockLevel)
                    return _entries[i];
            }
            return null;
        }
    }

    [Serializable]
    public class UnlockProgressData {
        public int UnlockLevel;
        public LocalizationKey TitleLocalizationKey;
    }
}
