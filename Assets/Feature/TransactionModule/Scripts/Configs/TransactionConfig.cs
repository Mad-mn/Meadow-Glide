using System;
using System.Collections.Generic;
using Feature.PlayerInventoryModule.Scripts;
using UnityEngine;

namespace Feature.TransactionModule.Scripts.Configs
{
    [Serializable]
    public class ResourceCost
    {
        public ResourceType Type;
        public int Amount;
    }

    [Serializable]
    public class ResourceReward
    {
        public ResourceType Type;
        public int Amount;
    }

    [CreateAssetMenu(fileName = "TransactionConfig", menuName = "Configs/Transaction/TransactionConfig")]
    public class TransactionConfig : ScriptableObject
    {
        public TransactionId TransactionId;
        public string FailureMessage;
        public List<ResourceCost> Costs;
        public List<ResourceReward> Rewards;
    }
}
