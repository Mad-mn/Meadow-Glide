using System;
using System.Collections.Generic;
using Feature.LevelModule.Scripts;
using Feature.StarModule.Scripts;
using Feature.TransactionModule.Scripts;
using UnityEngine;

namespace Feature.ChallengeModule.Scripts {
    [Serializable]
    public class StarRewardEntry {
        public StarRating RequiredStars;
        public ResourceAmount[] Rewards;
    }

    [CreateAssetMenu(fileName = "ChallengeConfig", menuName = "Configs/Challenge/ChallengeConfig")]
    public class ChallengeConfig : ScriptableObject {
        [SerializeField] private ChallengeType _challengeType;
        [SerializeField] private int _unlockLevel;
        [SerializeField] private List<LevelConfig> _levelPool = new List<LevelConfig>();
        [SerializeField] private StarRewardEntry[] _starRewards = new StarRewardEntry[0];

        public ChallengeType ChallengeType => _challengeType;
        public int UnlockLevel => _unlockLevel;
        public IReadOnlyList<LevelConfig> LevelPool => _levelPool;
        public StarRewardEntry[] StarRewards => _starRewards;
    }
}
