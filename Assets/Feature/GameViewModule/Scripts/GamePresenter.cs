using Cysharp.Threading.Tasks;
using Feature.ChallengeModule.Scripts;
using Feature.ConfirmExitToMainMenuViewModule.Scripts;
using Feature.LevelInitializeModule;
using Feature.LevelModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.MainTutorialViewModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SettingsViewModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.ToolButtonViewModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.UndoModule.Scripts;

namespace Feature.GameViewModule.Scripts {
    public class GamePresenter : PresenterBase<GameView> {
        private readonly IViewService _viewService;
        private readonly SaveDataModel _saveDataModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IAudioService _audioService;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IChallengeService _challengeService;
        private readonly ILocalizationService _localizationService;
        private readonly LevelModel _levelModel;

        public GamePresenter(GameView view, IViewService viewService, SaveDataModel saveDataModel,
            MoveTrackModel moveTrackModel, IAudioService audioService, ILevelInitializeService levelInitializeService,
            IChallengeService challengeService, ILocalizationService localizationService, LevelModel levelModel) : base(view) {
            _viewService = viewService;
            _saveDataModel = saveDataModel;
            _moveTrackModel = moveTrackModel;
            _audioService = audioService;
            _levelInitializeService = levelInitializeService;
            _challengeService = challengeService;
            _localizationService = localizationService;
            _levelModel = levelModel;
        }

        public override void Initialize() {
            View.MainMenuButton.onClick.AddListener(ShowConfirmExitToMainMenu);
            View.HelpButton.onClick.AddListener(ShowHelpView);
            View.ResetLevelButton.onClick.AddListener(ResetLevelButtonClicked);
            View.SettingsButton.onClick.AddListener(SettingsButtonClicked);
            _moveTrackModel.OnMovesChanged += UpdateMovesChangedsText;
            _viewService.PrewarmView<ToolButtonView>(ViewType.ToolButtonView);
            LocalizationEvents.OnLanguageChanged += SetupText;
        }

        private void SettingsButtonClicked() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.ShowView<SettingsView>(ViewType.SettingsView);
        }

        private void ShowHelpView() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.ShowView<MainTutorialView>(ViewType.MainTutorialView);
        }

        public override void Dispose() {
            base.Dispose();
            _viewService.ReleasePrewarmedView(ViewType.ToolButtonView);
            LocalizationEvents.OnLanguageChanged -= SetupText;
        }

        public override void Show() {
            _viewService.ShowView<ToolButtonView>(ViewType.ToolButtonView);
            SetupText();
        }

        public override void Hide() {
            base.Hide();
            _viewService.HideView(ViewType.ToolButtonView);
        }

        private void ResetLevelButtonClicked() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _levelInitializeService.ReloadScene().Forget();
        }

        private void SetupText() {
            string lvlText = _challengeService.IsActive ?
                _localizationService.Get(LocalizationKey.DailyChallenge_Title) :
                $"{_localizationService.Get(LocalizationKey.Global_Level)} {_levelModel.ReplayLevel ?? _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level}";
            View.SetLevelText(lvlText);
            UpdateMovesChangedsText();
        }

        private void UpdateMovesChangedsText() {
            View.SetMoveCount(_moveTrackModel.MovesLeft.ToString());
        }

        private void ShowConfirmExitToMainMenu() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.ShowView<ConfirmExitToMainMenuView>(ViewType.ConfirmExitToMainMenuView);
        }
    }
}