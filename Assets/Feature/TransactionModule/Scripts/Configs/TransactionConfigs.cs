using System.Collections.Generic;
using UnityEngine;

namespace Feature.TransactionModule.Scripts.Configs
{
    [CreateAssetMenu(fileName = "TransactionConfigs", menuName = "Configs/Transaction/TransactionConfigs")]
    public class TransactionConfigs : ScriptableObject
    {
        [SerializeField] private List<TransactionConfig> _configs;

        public IReadOnlyList<TransactionConfig> Configs => _configs;
    }
}
