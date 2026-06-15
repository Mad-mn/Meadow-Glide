using System;
using System.Collections.Generic;

namespace Feature.PlayerInventoryModule.Scripts
{
    public class PlayerInventoryModel
    {
        public event Action<ResourceType, int> OnBalanceChanged;
        public bool IsLoaded { get; private set; }

        private Dictionary<ResourceType, int> _balances = new Dictionary<ResourceType, int>();

        public int GetBalance(ResourceType type)
        {
            return _balances.TryGetValue(type, out var amount) ? amount : 0;
        }

        public void SetBalance(ResourceType type, int amount)
        {
            _balances[type] = amount;
            OnBalanceChanged?.Invoke(type, amount);
        }

        public Dictionary<ResourceType, int> GetAll()
        {
            return new Dictionary<ResourceType, int>(_balances);
        }

        public void LoadFrom(Dictionary<ResourceType, int> source)
        {
            _balances = new Dictionary<ResourceType, int>(source);
            IsLoaded = true;
        }
    }
}
