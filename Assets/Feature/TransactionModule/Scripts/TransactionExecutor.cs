using System.Collections.Generic;
using Feature.PlayerInventoryModule.Scripts;
using Feature.TransactionModule.Scripts.Configs;

namespace Feature.TransactionModule.Scripts
{
    public class TransactionExecutor
    {
        public void ApplyCosts(TransactionConfig config, IPlayerInventoryService storage)
        {
            if (config.Costs == null) return;

            foreach (var cost in config.Costs)
            {
                storage.TrySpend(cost.Type, cost.Amount);
            }
        }

        public List<ResourceAmount> ApplyRewards(TransactionConfig config, IPlayerInventoryService storage)
        {
            var appliedRewards = new List<ResourceAmount>();

            if (config.Rewards == null) return appliedRewards;

            foreach (var reward in config.Rewards)
            {
                storage.Add(reward.Type, reward.Amount);
                appliedRewards.Add(new ResourceAmount(reward.Type, reward.Amount));
            }

            return appliedRewards;
        }
    }
}
