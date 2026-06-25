using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.PlayerInventoryModule.Scripts {
    [Serializable]
    public class ResourceInfo {
        [SerializeField] private ResourceType _resourceType;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;

        public ResourceType ResourceType => _resourceType;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
    }

    [CreateAssetMenu(fileName = "ResourceInfoConfig", menuName = "Configs/ResourceInfo/ResourceInfoConfig")]
    public class ResourceInfoConfig : ScriptableObject {
        [SerializeField] private List<ResourceInfo> _resources = new List<ResourceInfo>();

        public IReadOnlyList<ResourceInfo> Resources => _resources;

        public ResourceInfo GetByType(ResourceType type) {
            foreach (ResourceInfo info in _resources) {
                if (info.ResourceType == type)
                    return info;
            }
            return null;
        }
    }
}
