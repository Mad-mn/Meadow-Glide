using Feature.DebugViewModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SettingsViewModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using AudioType = Feature.SoundModule.Scripts.AudioType;

namespace Feature.MainMenuViewModule.Scripts {
    public class MainMenuPresenter : PresenterBase<MainMenuView> {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly SaveDataModel _saveDataModel;
        private readonly IViewService _viewService;
        private readonly IAudioService _audioService;

        public MainMenuPresenter(MainMenuView view, IGameStateMachine gameStateMachine, SaveDataModel saveDataModel,
            IViewService viewService, IAudioService audioService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _saveDataModel = saveDataModel;
            _viewService = viewService;
            _audioService = audioService;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(StartSimpleGame);
            View.DebugButton.onClick.AddListener(ShowDebugWindow);
            View.SettingsButton.onClick.AddListener(OnSettingsClick);
        }

        public override void Show() {
            base.Show();
            SetupText();
        }

        private void OnSettingsClick() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.ShowView<SettingsView>(ViewType.SettingsView);
        }

        private void ShowDebugWindow() {
            _viewService.ShowView<DebugView>(ViewType.DebugView);
        }

        private void SetupText() {
            View.LevelText($"LEVEL {_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level}");
        }

        private void StartSimpleGame() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _gameStateMachine.EnterState(typeof(GameSimpleState));
        }
    }
}