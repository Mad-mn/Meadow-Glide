using System;
using System.Collections.Generic;
using Feature.LevelModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.StarModule.Scripts;
using Feature.TransactionModule.Scripts;
using UnityEngine;

namespace Feature.ChallengeModule.Scripts {
    public class ChallengeService : IChallengeService {
        private readonly IChallengeConfigProvider _configProvider;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IPlayerInventoryService _inventoryService;
        private readonly IStarCalculator _starCalculator;

        private readonly ChallengeSessionData _session = new ChallengeSessionData();

        public bool IsActive => _session.IsActive;

        public ChallengeService(
            IChallengeConfigProvider configProvider,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService,
            IPlayerInventoryService inventoryService,
            IStarCalculator starCalculator) {
            _configProvider = configProvider;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _inventoryService = inventoryService;
            _starCalculator = starCalculator;
        }

        public bool IsDailyChallengeAvailable(int currentLevel) {
            ChallengeConfig config = _configProvider.GetConfig(ChallengeType.Daily);
            if (config == null)
                return false;

            return currentLevel >= config.UnlockLevel;
        }

        public LevelData GetCurrentLevel() {
            if (!_session.IsActive)
                return default;

            ChallengeConfig config = _configProvider.GetConfig(_session.ActiveChallengeType);
            if (config == null)
                return default;

            ChallengeLevelSelector selector = new ChallengeLevelSelector(config);
            LevelData levelData = selector.GetLevelForDate(DateTime.Today);

            return new LevelData {
                LevelID = levelData.LevelID,
                LevelConfig = levelData.LevelConfig
            };
        }

        public void ActivateDailyChallenge(LevelConfig levelConfig) {
            _session.IsActive = true;
            _session.ActiveChallengeType = ChallengeType.Daily;
            _session.CurrentLevelConfig = levelConfig;
            _session.StarsEarned = 0;
            _session.IsCompleted = false;
        }

        public void OnChallengeCompleted(int maxMoves, int movesUsed) {
            if (!_session.IsActive)
                return;

            _session.StarsEarned = (int)_starCalculator.Calculate(maxMoves, movesUsed);
            _session.IsCompleted = true;

            DailyChallengeData data = GetOrCreateDailyData();

            if (_session.StarsEarned > data.TodayStarsEarned) {
                data.TodayStarsEarned = _session.StarsEarned;
            }

            SaveDailyData(data);
        }

        public int GetTodayStars() {
            DailyChallengeData data = GetOrCreateDailyData();
            return data.TodayStarsEarned;
        }

        public bool CanPlayToday() {
            DailyChallengeData data = GetOrCreateDailyData();
            return data.TodayStarsEarned < (int)StarRating.Three;
        }

        public bool CanClaimReward() {
            DailyChallengeData data = GetOrCreateDailyData();
            return data.TodayStarsEarned > data.ClaimedStarsThreshold;
        }

        public List<ResourceAmount> ClaimReward() {
            DailyChallengeData data = GetOrCreateDailyData();
            List<ResourceAmount> claimed = new List<ResourceAmount>();

            ChallengeConfig config = _configProvider.GetConfig(ChallengeType.Daily);
            if (config == null)
                return claimed;

            for (int stars = data.ClaimedStarsThreshold + 1; stars <= data.TodayStarsEarned; stars++) {
                StarRewardEntry reward = FindRewardEntry(config, (StarRating)stars);
                if (reward == null)
                    continue;

                foreach (ResourceAmount amount in reward.Rewards) {
                    _inventoryService.Add(amount.Type, amount.Amount);
                    claimed.Add(amount);
                }
            }

            data.ClaimedStarsThreshold = data.TodayStarsEarned;
            SaveDailyData(data);

            return claimed;
        }

        public void Deactivate() {
            _session.IsActive = false;
            _session.ActiveChallengeType = ChallengeType.Daily;
            _session.CurrentLevelConfig = null;
            _session.StarsEarned = 0;
            _session.IsCompleted = false;
        }

        private DailyChallengeData GetOrCreateDailyData() {
            DailyChallengeData data = _saveDataModel.Get<DailyChallengeData>(SaveDataType.DailyChallenge);
            if (data == null) {
                data = new DailyChallengeData();
            }

            string today = DateTime.Today.ToString("yyyy-MM-dd");
            if (data.LastCompletedDate != today) {
                data.LastCompletedDate = today;
                data.TodayStarsEarned = 0;
                data.ClaimedStarsThreshold = 0;
            }

            return data;
        }

        private void SaveDailyData(DailyChallengeData data) {
            _saveDataModel.Set(SaveDataType.DailyChallenge, data);
            _saveDataService.Save(SaveDataType.DailyChallenge);
        }

        private static StarRewardEntry FindRewardEntry(ChallengeConfig config, StarRating stars) {
            if (config.StarRewards == null)
                return null;

            foreach (StarRewardEntry entry in config.StarRewards) {
                if (entry.RequiredStars == stars)
                    return entry;
            }

            return null;
        }
    }
}
