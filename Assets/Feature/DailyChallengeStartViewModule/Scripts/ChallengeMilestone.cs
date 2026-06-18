using System.Collections.Generic;
using Feature.ChallengeModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
using Feature.TransactionModule.Scripts;
using UnityEngine;

namespace Feature.DailyChallengeStartViewModule.Scripts {
    public class ChallengeMilestone : MonoBehaviour {
        [SerializeField] private Transform _rewardsContainer;
        [SerializeField] private ChallengeRewardView _rewardPrefab;
        [SerializeField] private GameObject _checkmark;

        private readonly List<ChallengeRewardView> _spawnedRewards = new List<ChallengeRewardView>();

        public void Setup(StarRewardEntry rewardEntry, IResourceInfoProvider resourceInfoProvider, bool isComplited) {
            Clear();

            if (rewardEntry == null || rewardEntry.Rewards == null)
                return;

            _checkmark.SetActive(isComplited);
            foreach (ResourceAmount reward in rewardEntry.Rewards) {
                ResourceInfo info = resourceInfoProvider.GetInfo(reward.Type);
                Sprite icon = info != null ? info.Icon : null;

                ChallengeRewardView rewardView = Instantiate(_rewardPrefab, _rewardsContainer);
                rewardView.Setup(icon, reward.Amount);
                _spawnedRewards.Add(rewardView);
            }
        }

        public void Clear() {
            foreach (ChallengeRewardView reward in _spawnedRewards) {
                if (reward != null)
                    Destroy(reward.gameObject);
            }
            _spawnedRewards.Clear();
        }
    }
}
