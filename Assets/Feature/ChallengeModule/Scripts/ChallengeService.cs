using System;
using System.Collections.Generic;
using Feature.LevelModule.Scripts;
using Feature.MoveEfficiencyModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TransactionModule.Scripts;

namespace Feature.ChallengeModule.Scripts {
    public class ChallengeService : IChallengeService {
        private readonly IChallengeConfigProvider _configProvider;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IPlayerInventoryService _inventoryService;

        private readonly ChallengeSessionData _session = new ChallengeSessionData();

        public bool IsActive => _session.IsActive;

        public ChallengeService(
            IChallengeConfigProvider configProvider,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService,
            IPlayerInventoryService inventoryService) {
            _configProvider = configProvider;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _inventoryService = inventoryService;
        }

        public bool IsDailyChallengeAvailable() {
            int playerLevel = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level;
            ChallengeConfig config = _configProvider.GetConfig(ChallengeType.Daily);
            if (config == null)
                return false;

            return playerLevel >= config.UnlockLevel;
        }

        public LevelData GetCurrentLevel() {
            LevelData levelData = ResolveDailyLevel();
            if (levelData.LevelConfig != null)
                _session.CurrentLevelConfig = levelData.LevelConfig;

            return levelData;
        }

        public int GetMinMoves() {
            if (_session.CurrentLevelConfig != null)
                return _session.CurrentLevelConfig.ShortestSolution;

            LevelData levelData = ResolveDailyLevel();
            return levelData.LevelConfig != null ? levelData.LevelConfig.ShortestSolution : 0;
        }

        public void ActivateDailyChallenge(LevelConfig levelConfig) {
            _session.IsActive = true;
            _session.ActiveChallengeType = ChallengeType.Daily;
            _session.CurrentLevelConfig = levelConfig ?? ResolveDailyLevel().LevelConfig;
            _session.CurrentResult = MoveEfficiencyResult.None;
            _session.IsCompleted = false;
        }

        private LevelData ResolveDailyLevel() {
            ChallengeConfig config = _configProvider.GetConfig(ChallengeType.Daily);
            if (config == null)
                return default;

            ChallengeLevelSelector selector = new ChallengeLevelSelector(config);
            return selector.GetLevelForDate(DateTime.Today);
        }

        public void OnChallengeCompleted(MoveEfficiencyResult result) {
            if (!_session.IsActive)
                return;

            _session.CurrentResult = result;
            _session.IsCompleted = true;

            DailyChallengeData data = GetOrCreateDailyData();
            _session.PreviousClaimedResult = (MoveEfficiencyResult)data.ClaimedResultThreshold;

            if ((int)result > data.TodayBestResult) {
                data.TodayBestResult = (int)result;
            }

            ClaimNewRewards(data);
            SaveDailyData(data);
        }

        public MoveEfficiencyResult GetTodayBestResult() {
            DailyChallengeData data = GetOrCreateDailyData();
            return (MoveEfficiencyResult)data.TodayBestResult;
        }

        public MoveEfficiencyResult GetPreviousClaimedResult() {
            return _session.PreviousClaimedResult;
        }

        public bool CanPlayToday() {
            DailyChallengeData data = GetOrCreateDailyData();
            return data.TodayBestResult < (int)MoveEfficiencyResult.PerfectClear;
        }

        public bool CanClaimReward() {
            DailyChallengeData data = GetOrCreateDailyData();
            return data.TodayBestResult > data.ClaimedResultThreshold;
        }

        public List<ResourceAmount> ClaimReward() {
            DailyChallengeData data = GetOrCreateDailyData();
            List<ResourceAmount> claimed = ClaimNewRewards(data);
            SaveDailyData(data);
            return claimed;
        }

        private List<ResourceAmount> ClaimNewRewards(DailyChallengeData data) {
            List<ResourceAmount> claimed = new List<ResourceAmount>();

            ChallengeConfig config = _configProvider.GetConfig(ChallengeType.Daily);
            if (config == null)
                return claimed;

            for (int result = data.ClaimedResultThreshold + 1; result <= data.TodayBestResult; result++) {
                ChallengeRewardEntry reward = FindRewardEntry(config, (MoveEfficiencyResult)result);
                if (reward == null)
                    continue;

                foreach (ResourceAmount amount in reward.Rewards) {
                    _inventoryService.Add(amount.Type, amount.Amount);
                    claimed.Add(amount);
                }
            }

            data.ClaimedResultThreshold = data.TodayBestResult;
            return claimed;
        }

        public void Deactivate() {
            _session.IsActive = false;
            _session.ActiveChallengeType = ChallengeType.Daily;
            _session.CurrentLevelConfig = null;
            _session.CurrentResult = MoveEfficiencyResult.None;
            _session.PreviousClaimedResult = MoveEfficiencyResult.None;
            _session.IsCompleted = false;
        }

        public TimeSpan GetTimeUntilNextDay() {
            DateTime now = DateTime.Now;
            DateTime tomorrow = DateTime.Today.AddDays(1);
            return tomorrow - now;
        }

        private DailyChallengeData GetOrCreateDailyData() {
            DailyChallengeData data = _saveDataModel.Get<DailyChallengeData>(SaveDataType.DailyChallenge);
            if (data == null) {
                data = new DailyChallengeData();
            }

            string today = DateTime.Today.ToString("yyyy-MM-dd");
            if (data.LastCompletedDate != today) {
                data.LastCompletedDate = today;
                data.TodayBestResult = 0;
                data.ClaimedResultThreshold = 0;
            }

            return data;
        }

        private void SaveDailyData(DailyChallengeData data) {
            _saveDataModel.Set(SaveDataType.DailyChallenge, data);
            _saveDataService.Save(SaveDataType.DailyChallenge);
        }

        private static ChallengeRewardEntry FindRewardEntry(ChallengeConfig config, MoveEfficiencyResult result) {
            if (config.Rewards == null)
                return null;

            foreach (ChallengeRewardEntry entry in config.Rewards) {
                if (entry.RequiredResult == result)
                    return entry;
            }

            return null;
        }
    }
}
