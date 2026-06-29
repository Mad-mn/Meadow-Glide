using System.Linq;
using Feature.DailyChallengeStartViewModule.Scripts;
using Feature.DebugViewModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.MessageViewModule.Scripts;
using Feature.PerfectMapViewModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SettingsViewModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.TransactionModule.Scripts;
using Feature.UIServiceModule.Scripts;
using JetBrains.Annotations;
using UnityEngine;
using AudioType = Feature.SoundModule.Scripts.AudioType;

namespace Feature.MainMenuViewModule.Scripts {
    public class MainMenuPresenter : PresenterBase<MainMenuView> {
        private const int PERFECT_CHALLENGE_UNLOCK_LEVEL = 30;
        private const int DAILY_CHALLENGE_UNLOCK_LEVEL = 12;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly SaveDataModel _saveDataModel;
        private readonly IViewService _viewService;
        private readonly IAudioService _audioService;
        private readonly IPlayerInventoryService _playerInventoryService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageService _messageService;
        private readonly IPerfectMapService _perfectMapService;
        private readonly PerfectMapModel _perfectMapModel;

        public MainMenuPresenter(MainMenuView view, IGameStateMachine gameStateMachine, SaveDataModel saveDataModel, IViewService viewService,
            IAudioService audioService, [CanBeNull] IPlayerInventoryService playerInventoryService, ILocalizationService localizationService,
            IMessageService messageService, IPerfectMapService perfectMapService, PerfectMapModel perfectMapModel) : base(view) {
            _gameStateMachine = gameStateMachine;
            _saveDataModel = saveDataModel;
            _viewService = viewService;
            _audioService = audioService;
            _playerInventoryService = playerInventoryService;
            _localizationService = localizationService;
            _messageService = messageService;
            _perfectMapService = perfectMapService;
            _perfectMapModel = perfectMapModel;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(StartSimpleGame);
            View.DebugButton.onClick.AddListener(ShowDebugWindow);
            View.SettingsButton.onClick.AddListener(OnSettingsClick);
            View.PerfectChallengeButton.onClick.AddListener(OnPerfectChallenge);
            View.DailyChallengeButton.onClick.AddListener(OnDailyChallengeClick);
            _perfectMapModel.OnRewardClaimed += OnChangePerfectMap;
            LocalizationEvents.OnLanguageChanged += SetupText;
        }

        public override void Show() {
            base.Show();
            SetupText();
            UpdateCoinsCountTxt();
            UpdateChallengeInfo();
        }

        private void OnPerfectChallenge() {
            if (_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress)
                    .Level < PERFECT_CHALLENGE_UNLOCK_LEVEL) {
                _messageService.Show(LocalizationKey.PerfectChallenge_Locked);
                return;
            }
            _viewService.ShowView<PerfectMapView>(ViewType.PerfectMapView);
        }

        private void OnChangePerfectMap(int level) =>
            UpdateChallengeInfo();

        private void UpdateChallengeInfo() {
            PlayerProgressData data = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            int level = data.Level;
            View.DailyChallengeLock.SetActive(level < DAILY_CHALLENGE_UNLOCK_LEVEL);
            View.PerfectChallengeLock.SetActive(level < PERFECT_CHALLENGE_UNLOCK_LEVEL);
            View.PerfectChallengeNotification.SetActive(level >= PERFECT_CHALLENGE_UNLOCK_LEVEL && _perfectMapService.HasUnclaimedRewards());
        }

        private void UpdateCoinsCountTxt() {
            View.CoinsCountText.text = _playerInventoryService.GetBalance(ResourceType.Coins)
                .ToString();
        }

        private void OnSettingsClick() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.ShowView<SettingsView>(ViewType.SettingsView);
        }

        private void ShowDebugWindow() {
            _viewService.ShowView<DebugView>(ViewType.DebugView);
        }

        private void OnDailyChallengeClick() {
            if (_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress)
                    .Level < DAILY_CHALLENGE_UNLOCK_LEVEL) {
                _messageService.Show(LocalizationKey.DailyChallenge_Locked);
                return;
            }
            _viewService.ShowView<DailyChallengeStartView>(ViewType.DailyChallengeStartView);
        }

        private void SetupText() {
            View.LevelText($"{_localizationService.Get(LocalizationKey.Global_Level)} {_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level}");
        }

        private void StartSimpleGame() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _gameStateMachine.EnterState(typeof(GameSimpleState));
        }
    }
}