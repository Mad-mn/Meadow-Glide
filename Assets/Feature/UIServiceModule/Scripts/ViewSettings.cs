using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.UIServiceModule.Scripts {
    [CreateAssetMenu(fileName = "ViewSettings", menuName = "UI/ViewSettings")]
    public class ViewSettings : ScriptableObject {
        [SerializeField] private List<ViewConfigEntry> _entries;

        public IEnumerable<ViewConfigEntry> Entries => _entries;

        [Serializable]
        public class ViewConfigEntry {
            public ViewType ViewType;
            public string Address;
            public string PresenterTypeName; // Use string for serialization if needed, or handle differently
        }
    }
}