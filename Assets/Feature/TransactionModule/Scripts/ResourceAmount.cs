using System;
using Feature.PlayerInventoryModule.Scripts;

namespace Feature.TransactionModule.Scripts
{
    [Serializable]
    public struct ResourceAmount
    {
        public ResourceType Type;
        public int Amount;

        public ResourceAmount(ResourceType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }
}
