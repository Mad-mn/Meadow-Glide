using System.Collections.Generic;
using Feature.TransactionModule.Scripts.Configs;

namespace Feature.TransactionModule.Scripts
{
    public class TransactionExecutor
    {
        public void ApplyCosts(TransactionConfig config, IResourceStorage storage)
        {
            if (config.Costs == null) return;

            foreach (var cost in config.Costs)
            {
                storage.Spend(cost.Type, cost.Amount);
            }
        }

        public List<ResourceAmount> ApplyRewards(TransactionConfig config, IResourceStorage storage)
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
