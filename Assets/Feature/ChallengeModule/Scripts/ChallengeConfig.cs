using System;
using System.Collections.Generic;
using Feature.LevelModule.Scripts;
using Feature.MoveEfficiencyModule.Scripts;
using Feature.TransactionModule.Scripts;
using UnityEngine;

namespace Feature.ChallengeModule.Scripts {
    [Serializable]
    public class ChallengeRewardEntry {
        public MoveEfficiencyResult RequiredResult;
        public ResourceAmount[] Rewards;
    }

    [CreateAssetMenu(fileName = "ChallengeConfig", menuName = "Configs/Challenge/ChallengeConfig")]
    public class ChallengeConfig : ScriptableObject {
        [SerializeField] private ChallengeType _challengeType;
        [SerializeField] private int _unlockLevel;
        [SerializeField] private List<LevelConfig> _levelPool = new List<LevelConfig>();
        [SerializeField] private ChallengeRewardEntry[] _rewards = new ChallengeRewardEntry[0];

        public ChallengeType ChallengeType => _challengeType;
        public int UnlockLevel => _unlockLevel;
        public IReadOnlyList<LevelConfig> LevelPool => _levelPool;
        public ChallengeRewardEntry[] Rewards => _rewards;
    }
}
