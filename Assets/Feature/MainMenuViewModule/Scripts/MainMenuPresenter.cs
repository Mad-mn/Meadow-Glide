using Feature.DailyChallengeStartViewModule.Scripts;
using Feature.DebugViewModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
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
        private readonly IGameStateMachine _gameStateMachine;
        private readonly SaveDataModel _saveDataModel;
        private readonly IViewService _viewService;
        private readonly IAudioService _audioService;
        private readonly IPlayerInventoryService _playerInventoryService;
        private readonly ILocalizationService _localizationService;

        public MainMenuPresenter(MainMenuView view, IGameStateMachine gameStateMachine, SaveDataModel saveDataModel, IViewService viewService,
            IAudioService audioService, [CanBeNull] IPlayerInventoryService playerInventoryService, ILocalizationService localizationService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _saveDataModel = saveDataModel;
            _viewService = viewService;
            _audioService = audioService;
            _playerInventoryService = playerInventoryService;
            _localizationService = localizationService;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(StartSimpleGame);
            View.DebugButton.onClick.AddListener(ShowDebugWindow);
            View.SettingsButton.onClick.AddListener(OnSettingsClick);
            View.DailyChallengeButton.onClick.AddListener(OnDailyChallengeClick);
            LocalizationEvents.OnLanguageChanged += SetupText;
        }

        public override void Show() {
            base.Show();
            SetupText();
            UpdateCoinsCountTxt();
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
            _viewService.ShowView<DailyChallengeStartView>(ViewType.DailyChallengeStartView);
        }

        private void SetupText() {
            View.LevelText($"{_localizationService.Get(LocalizationKey.Global_Level)} {_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level}");
        }

        private void StartSimpleGame() {
            
            _audioService.PlaySound(AudioType.ButtonClick);
            _gameStateMachine.EnterState(typeof(GameSimpleState));
            throw new System.Exception("test exception please ignore");
        }
    }
}