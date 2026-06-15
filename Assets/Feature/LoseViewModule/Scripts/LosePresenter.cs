using Cysharp.Threading.Tasks;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.InputModule.Scripts;
using Feature.LevelInitializeModule;
using Feature.LevelModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;

namespace Feature.LoseViewModule.Scripts {
    public class LosePresenter : PresenterBase<LoseView> {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IViewService _viewService;
        private readonly IMoveTrackService _moveTrackService;
        private readonly IInteractionStateService _interactionStateService;

        public LosePresenter(LoseView view, IGameStateMachine gameStateMachine, ILevelInitializeService levelInitializeService,
            IViewService viewService, IMoveTrackService moveTrackService, IInteractionStateService interactionStateService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _levelInitializeService = levelInitializeService;
            _viewService = viewService;
            _moveTrackService = moveTrackService;
            _interactionStateService = interactionStateService;
        }

        public override void Initialize() {
            View.RestartButton.onClick.AddListener(OnNextButtonClick);
            View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
            View.AddMovesButton.onClick.AddListener(AddMovesButtonClick);
        }

        public override void Show() {
            base.Show();
            _interactionStateService.BlockInput();
        }

        public override void Hide() {
            base.Hide();
            _interactionStateService.UnblockInput();
        }

        private void AddMovesButtonClick() {
            _viewService.HideView(ViewType.LoseView);
            _moveTrackService.AddMoves(5);
        }

        private void OnMainMenuButtonClick() {
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }

        private void OnNextButtonClick() {
            _levelInitializeService.ReloadScene().Forget();
        }
    }
}