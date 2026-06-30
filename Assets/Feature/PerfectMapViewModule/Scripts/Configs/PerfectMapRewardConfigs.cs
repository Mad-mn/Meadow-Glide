using System.Collections.Generic;
using UnityEngine;

namespace Feature.PerfectMapViewModule.Scripts.Configs {
    [CreateAssetMenu(fileName = "PerfectMapRewardConfigs", menuName = "Configs/PerfectMap/PerfectMapRewardConfigs")]
    public class PerfectMapRewardConfigs : ScriptableObject {
        [SerializeField] private List<PerfectMapRewardConfig> _configs = new List<PerfectMapRewardConfig>();

        public IReadOnlyList<PerfectMapRewardConfig> Configs => _configs;
    }
}
