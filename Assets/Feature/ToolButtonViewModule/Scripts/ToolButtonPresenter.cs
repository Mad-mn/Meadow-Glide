using Feature.ToolModule.Scripts;
using Feature.TransactionModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.ToolButtonViewModule.Scripts {
    public class ToolButtonPresenter : PresenterBase<ToolButtonView> {
        private readonly IToolService _toolService;
        private readonly IPriceDataProvider _priceDataProvider;

        public ToolButtonPresenter(ToolButtonView view, IToolService toolService, IPriceDataProvider priceDataProvider) : base(view) {
            _toolService = toolService;
            _priceDataProvider = priceDataProvider;
        }

        public override void Initialize() {
            foreach (ToolButton toolButton in View.ToolButtons) {
                toolButton.OnButtonClick += OnButtonClick;
            }
        }

        public override void Show() {
            base.Show();
            SetupView();
        }

        private void OnButtonClick(ToolType toolType) {
            if (CheckForLock(toolType))
                return;

            _toolService.ExecuteTool(toolType);
        }

        private void SetupView() {
            foreach (ToolButton toolButton in View.ToolButtons) {
                toolButton.SetupView(GetToolButtonViewData(toolButton.ToolType));
            }
        }

        private ToolButtonViewData GetToolButtonViewData(ToolType toolButtonToolType) {
            return new ToolButtonViewData() {
                HasTool = _toolService.HasTool(toolButtonToolType), 
                Amount = _toolService.GetToolAmount(toolButtonToolType),
                Price = _priceDataProvider.GetPrice(TransactionId.BuyUndo),
                Blocked = CheckForLock(toolButtonToolType)
            };
        }

        private bool CheckForLock(ToolType toolType) {
            return !_toolService.CanUseTool(toolType);
        }
    }
}