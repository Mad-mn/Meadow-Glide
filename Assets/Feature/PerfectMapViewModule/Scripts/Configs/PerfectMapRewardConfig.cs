using System;
using Feature.PlayerInventoryModule.Scripts;
using UnityEngine;

namespace Feature.PerfectMapViewModule.Scripts.Configs {
    [Serializable]
    public class PerfectMapRewardConfig {
        [SerializeField] private int _fromLevel;
        [SerializeField] private int _toLevel;
        [SerializeField] private ResourceType _rewardType;
        [SerializeField] private int _rewardAmount;

        public int FromLevel => _fromLevel;
        public int ToLevel => _toLevel;
        public ResourceType RewardType => _rewardType;
        public int RewardAmount => _rewardAmount;
    }
}
