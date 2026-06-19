using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.PlayerInventoryModule.Scripts;
using Feature.TransactionModule.Scripts;
using Feature.TransactionModule.Scripts.Configs;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.ConfirmBuyViewModule.Scripts {
    public class ConfirmBuyPresenter : PresenterBase<ConfirmBuyView> {
        private readonly ITransactionService _transactionService;
        private readonly IPriceDataProvider _priceDataProvider;
        private readonly ITransactionConfigsProvider _configsProvider;
        private readonly IViewService _viewService;
        private readonly ConfirmBuyViewModel _viewModel;
        private readonly IPlayerInventoryService _playerInventoryService;
        private readonly IInteractionStateService _interactionStateService;
        private readonly ILocalizationService _localizationService;
        private TransactionId _transactionId;

        public ConfirmBuyPresenter(ConfirmBuyView view, ITransactionService transactionService, IPriceDataProvider priceDataProvider,
            ITransactionConfigsProvider configsProvider, IViewService viewService, ConfirmBuyViewModel viewModel,
            IPlayerInventoryService playerInventoryService,
            IInteractionStateService interactionStateService, ILocalizationService localizationService) : base(view) {
            _transactionService = transactionService;
            _priceDataProvider = priceDataProvider;
            _configsProvider = configsProvider;
            _viewService = viewService;
            _viewModel = viewModel;
            _playerInventoryService = playerInventoryService;
            _interactionStateService = interactionStateService;
            _localizationService = localizationService;
        }

        public override void Initialize() {
            _viewModel.OnSetupTransactionId += SetupTransactionId;
            View.NoButton.onClick.AddListener(OnCloseButtonClick);
            View.YesButton.onClick.AddListener(OnYesButtonClick);
        }

        public override void Dispose() {
            base.Dispose();
            _viewModel.OnSetupTransactionId -= SetupTransactionId;
        }

        public override void Show() {
            base.Show();
            _interactionStateService.BlockInput();
            View.PlayerCoinsAmountText.text = _playerInventoryService.GetBalance(ResourceType.Coins).ToString();
        }

        public override void Hide() {
            base.Hide();
            _interactionStateService.UnblockInput();
        }

        private void OnYesButtonClick() {
            var result = _transactionService.Execute(_transactionId);
            if (result.Success) {
                _viewModel.ConfirmSuccess();
                OnCloseButtonClick();
            }
        }

        private void OnCloseButtonClick() {
         _viewService.HideView(ViewType.ConfirmBuyView);   
        }

        private void SetupTransactionId(TransactionId transactionId) {
            _transactionId = transactionId;
            View.PriceText.text = _priceDataProvider.GetPrice(_transactionId).ToString();
            View.TitleText.text = GetTextForTitle();
        }

        private string GetTextForTitle() {
            TransactionConfig config = _configsProvider.GetConfig(_transactionId);
            if (config is null) {
                Debug.LogError($"Config for transaction if {_transactionId} does not exist");
                return string.Empty;
            }

            string title = $"{_localizationService.Get(LocalizationKey.Global_ConfirmBuy)} " +
                           $"{config.Rewards[0].Amount.ToString()}" +
                           $" {_localizationService.Get(config.Rewards[0].NameKey)}?";
            return title;
        }
    }
}