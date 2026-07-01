using Cysharp.Threading.Tasks;
using Feature.AnalyticsModule.Scripts;
using Feature.ConfirmBuyViewModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.InputModule.Scripts;
using Feature.LevelInitializeModule;
using Feature.LevelResultModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TrackMoveModule.Scripts;
using Feature.TransactionModule.Scripts;
using Feature.TransactionModule.Scripts.Configs;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;

namespace Feature.LoseViewModule.Scripts {
    public class LosePresenter : PresenterBase<LoseView> {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IViewService _viewService;
        private readonly IMoveTrackService _moveTrackService;
        private readonly IInteractionStateService _interactionStateService;
        private readonly ConfirmBuyViewModel _confirmBuyViewModel;
        private readonly ITransactionConfigsProvider _transactionConfigsProvider;
        private readonly ILocalizationService _localizationService;
        private readonly IAnalyticsService _analyticsService;
        private readonly LevelModel _levelModel;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ILevelResultService _levelResultService;

        public LosePresenter(LoseView view, IGameStateMachine gameStateMachine, ILevelInitializeService levelInitializeService,
            IViewService viewService, IMoveTrackService moveTrackService, IInteractionStateService interactionStateService,
            ConfirmBuyViewModel confirmBuyViewModel, ITransactionConfigsProvider transactionConfigsProvider, ILocalizationService localizationService,
            IAnalyticsService analyticsService, LevelModel levelModel, ISaveDataModel saveDataModel, ILevelResultService levelResultService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _levelInitializeService = levelInitializeService;
            _viewService = viewService;
            _moveTrackService = moveTrackService;
            _interactionStateService = interactionStateService;
            _confirmBuyViewModel = confirmBuyViewModel;
            _transactionConfigsProvider = transactionConfigsProvider;
            _localizationService = localizationService;
            _analyticsService = analyticsService;
            _levelModel = levelModel;
            _saveDataModel = saveDataModel;
            _levelResultService = levelResultService;
        }

        public override void Initialize() {
            View.RestartButton.onClick.AddListener(OnNextButtonClick);
            View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
            View.AddMovesButton.onClick.AddListener(AddMovesButtonClick);
        }

        public override void Show() {
            base.Show();
            _interactionStateService.BlockInput();
            SetupAddMovesButtonText();
        }

        private void SetupAddMovesButtonText() {
            TransactionConfig config = _transactionConfigsProvider.GetConfig(TransactionId.BuyExtraMoves);
            View.AddMovesButtonText.text = $"+ {config.Rewards[0].Amount}\n{_localizationService.Get(LocalizationKey.Global_Moves)}";
        }

        public override void Hide() {
            base.Hide();
            _interactionStateService.UnblockInput();
        }

        private void AddMovesButtonClick() {
            _viewService.ShowView<ConfirmBuyView>(ViewType.ConfirmBuyView);
            _confirmBuyViewModel.SetupTransactionId(TransactionId.BuyExtraMoves);
            _confirmBuyViewModel.OnConfirmSuccess += OnConfirmSuccess;

            void OnConfirmSuccess() {
                _confirmBuyViewModel.OnConfirmSuccess -= OnConfirmSuccess;
                _viewService.HideView(ViewType.LoseView);
                _moveTrackService.AddMoves(_transactionConfigsProvider.GetConfig(TransactionId.BuyExtraMoves).Rewards[0].Amount);
                _levelResultService.ResetLose();
                int levelId = _levelModel.ReplayLevel ?? _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level;
                _analyticsService.ExtraMovesPurchased(levelId);
            }
            
        }

        private void OnMainMenuButtonClick() {
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }

        private void OnNextButtonClick() {
            _levelInitializeService.ReloadScene().Forget();
        }
    }
}