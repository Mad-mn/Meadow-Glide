using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LevelModule.Scripts;
using Feature.MoveEfficiencyModule.Scripts;
using Feature.PerfectMapViewModule.Scripts.Configs;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.PerfectMapViewModule.Scripts {
    public class PerfectMapPresenter : PresenterBase<PerfectMapView> {
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly UniTask<LevelConfigProvider> _levelConfigProviderTask;
        private readonly IPerfectMapRewardConfigProvider _rewardConfigProvider;
        private readonly IPlayerInventoryService _inventoryService;
        private readonly IViewService _viewService;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IInstantiator _instantiator;
        private readonly LevelModel _levelModel;
        private readonly PerfectMapModel _perfectMapModel;

        private LevelConfigProvider _levelConfigProvider;
        private readonly List<LevelInfoPerfectChallenge> _spawnedItems = new List<LevelInfoPerfectChallenge>();

        public PerfectMapPresenter(
            PerfectMapView view,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService,
            UniTask<LevelConfigProvider> levelConfigProviderTask,
            IPerfectMapRewardConfigProvider rewardConfigProvider,
            IPlayerInventoryService inventoryService,
            IViewService viewService,
            IGameStateMachine gameStateMachine,
            IInstantiator instantiator,
            LevelModel levelModel,
            PerfectMapModel perfectMapModel) : base(view) {
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _levelConfigProviderTask = levelConfigProviderTask;
            _rewardConfigProvider = rewardConfigProvider;
            _inventoryService = inventoryService;
            _viewService = viewService;
            _gameStateMachine = gameStateMachine;
            _instantiator = instantiator;
            _levelModel = levelModel;
            _perfectMapModel = perfectMapModel;
        }

        public override void Initialize() {
            _levelConfigProviderTask.ContinueWith(provider => _levelConfigProvider = provider).Forget();
        }

        public override void Show() {
            base.Show();
            View.CloseButton.onClick.AddListener(Close);
            PopulateLevelList();
        }

        private void Close() {
            _viewService.HideView(ViewType.PerfectMapView);
        }

        public override void Hide() {
            base.Hide();
            View.CloseButton.onClick.RemoveListener(Close);
            ClearList();
        }

        private async void PopulateLevelList() {
            if (_levelConfigProvider == null)
                _levelConfigProvider = await _levelConfigProviderTask;

            ClearList();

            PlayerProgressData progress = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            int lastSpawnedIndex = -1;

            foreach (var kvp in _levelConfigProvider.LevelDatas) {
                int levelId = kvp.Key;
                if (levelId <= 0) continue;
                if (progress.CompletedLevels == null || !progress.CompletedLevels.ContainsKey(levelId)) continue;

                LevelData levelData = kvp.Value;
                LevelInfoPerfectData data = BuildLevelData(levelId, levelData, progress);
                LevelInfoPerfectChallenge item = _instantiator.InstantiatePrefabForComponent<LevelInfoPerfectChallenge>(View.InfoPrefab, View.InfoParent);
                item.Setup(data);

                int capturedLevel = levelId;
                item.PlayButton.onClick.AddListener(() => OnPlayClick(capturedLevel));
                item.ClaimButton.onClick.AddListener(() => OnClaimClick(capturedLevel, item));

                _spawnedItems.Add(item);
                lastSpawnedIndex = _spawnedItems.Count - 1;
            }

            if (lastSpawnedIndex >= 0) {
                Canvas.ForceUpdateCanvases();
                View.ScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private LevelInfoPerfectData BuildLevelData(int levelId, LevelData levelData, PlayerProgressData progress) {
            LevelInfoPerfectData data = new LevelInfoPerfectData {
                LevelNumber = levelId,
                ShortestSolution = levelData.LevelConfig.ShortestSolution,
                State = LevelPerfectState.NotCompleted
            };

            if (progress.CompletedLevels != null && progress.CompletedLevels.TryGetValue(levelId, out LevelCompletionData completionData)) {
                data.BestMoves = completionData.MovesUsed;

                if (completionData.Status == MoveEfficiencyResult.PerfectClear) {
                    if (progress.ClaimedPerfectMapRewards != null && progress.ClaimedPerfectMapRewards.Contains(levelId)) {
                        data.State = LevelPerfectState.PerfectClaimed;
                    } else {
                        data.State = LevelPerfectState.PerfectNotClaimed;
                        PerfectMapRewardConfig rewardConfig = _rewardConfigProvider.GetConfigForLevel(levelId);
                        if (rewardConfig != null) {
                            data.RewardType = rewardConfig.RewardType;
                            data.RewardAmount = rewardConfig.RewardAmount;
                        }
                    }
                } else {
                    data.State = LevelPerfectState.CompletedNotPerfect;
                }
            }

            return data;
        }

        private void OnPlayClick(int levelId) {
            _levelModel.ReplayLevel = levelId;
            _viewService.HideView(ViewType.PerfectMapView);
            _gameStateMachine.EnterState(typeof(GameSimpleState));
        }

        private void OnClaimClick(int levelId, LevelInfoPerfectChallenge item) {
            PerfectMapRewardConfig rewardConfig = _rewardConfigProvider.GetConfigForLevel(levelId);
            if (rewardConfig == null) return;

            _inventoryService.Add(rewardConfig.RewardType, rewardConfig.RewardAmount);

            PlayerProgressData progress = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            if (progress.ClaimedPerfectMapRewards == null)
                progress.ClaimedPerfectMapRewards = new HashSet<int>();
            progress.ClaimedPerfectMapRewards.Add(levelId);
            _saveDataService.Save(SaveDataType.PlayerProgress);

            LevelInfoPerfectData data = BuildLevelData(levelId, _levelConfigProvider.LevelDatas[levelId], progress);
            item.Setup(data);

            _perfectMapModel.ClaimReward(levelId);
        }

        private void ClearList() {
            foreach (LevelInfoPerfectChallenge item in _spawnedItems) {
                if (item != null)
                    Object.Destroy(item.gameObject);
            }
            _spawnedItems.Clear();
        }
    }
}
