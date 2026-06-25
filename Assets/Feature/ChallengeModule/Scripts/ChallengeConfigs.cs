using System.Collections.Generic;
using UnityEngine;

namespace Feature.ChallengeModule.Scripts {
    [CreateAssetMenu(fileName = "ChallengeConfigs", menuName = "Configs/Challenge/ChallengeConfigs")]
    public class ChallengeConfigs : ScriptableObject {
        [SerializeField] private List<ChallengeConfig> _configs = new List<ChallengeConfig>();

        public IReadOnlyList<ChallengeConfig> Configs => _configs;
    }
}
