using System;

namespace Feature.PerfectMapViewModule.Scripts {
    public class PerfectMapModel {
        public event Action<int> OnRewardClaimed;

        public void ClaimReward(int levelId) {
            OnRewardClaimed?.Invoke(levelId);
        }
    }
}
