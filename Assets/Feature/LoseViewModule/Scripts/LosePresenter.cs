using Cysharp.Threading.Tasks;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LevelInitializeModule;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;

namespace Feature.LoseViewModule.Scripts {
    public class LosePresenter : PresenterBase<LoseView> {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ILevelInitializeService _levelInitializeService;
        public LosePresenter(LoseView view, IGameStateMachine gameStateMachine, ILevelInitializeService levelInitializeService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _levelInitializeService = levelInitializeService;
        }

        public override void Initialize() {
            View.RestartButton.onClick.AddListener(OnNextButtonClick);
            View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        }

        private void OnMainMenuButtonClick() {
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }

        private void OnNextButtonClick() {
            _levelInitializeService.ReloadScene().Forget();
        }
    }
}