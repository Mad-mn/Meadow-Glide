using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LevelInitializeModule;
using Feature.UIServiceModule.Scripts;
using Cysharp.Threading.Tasks;

namespace Feature.WinLevelModule.Scripts {
    public class WinLevelPresenter : PresenterBase<WinLevel> {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ILevelInitializeService _levelInitializeService;
        public WinLevelPresenter(WinLevel view, IGameStateMachine gameStateMachine, ILevelInitializeService levelInitializeService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _levelInitializeService = levelInitializeService;
        }

        public override void Initialize() {
            View.NextButton.onClick.AddListener(OnNextButtonClick);
            View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        }

        private void OnMainMenuButtonClick() {
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }

        private void OnNextButtonClick() {
            _levelInitializeService.LoadNextLevel().Forget();
        }
    }
}