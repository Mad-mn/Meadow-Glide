using System;
using Feature.ConfirmBuyViewModule.Scripts;
using Feature.ToolModule.Scripts;
using Feature.TransactionModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.ToolButtonViewModule.Scripts {
    public class ToolButtonPresenter : PresenterBase<ToolButtonView> {
        private readonly IToolService _toolService;
        private readonly IPriceDataProvider _priceDataProvider;
        private readonly IViewService _viewService;
        private readonly ConfirmBuyViewModel _confirmBuyViewModel;

        public ToolButtonPresenter(ToolButtonView view, IToolService toolService, IPriceDataProvider priceDataProvider,
            IViewService viewService, ConfirmBuyViewModel confirmBuyViewModel) : base(view) {
            _toolService = toolService;
            _priceDataProvider = priceDataProvider;
            _viewService = viewService;
            _confirmBuyViewModel = confirmBuyViewModel;
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
            
            if(_toolService.HasTool(toolType)) {
                _toolService.ExecuteTool(toolType);
                SetupViewForTool(toolType);
            }
            else {
                _viewService.ShowView<ConfirmBuyView>(ViewType.ConfirmBuyView);
                _confirmBuyViewModel.SetupTransactionId(GetTransactionId(toolType));
                _confirmBuyViewModel.OnConfirmSuccess += OnConfirmSuccess;
            }

            void OnConfirmSuccess() {
                _confirmBuyViewModel.OnConfirmSuccess -= OnConfirmSuccess;
                SetupViewForTool(toolType);
            }
        }

        private void SetupView() {
            foreach (ToolButton toolButton in View.ToolButtons) {
                toolButton.SetupView(GetToolButtonViewData(toolButton.ToolType));
            }
        }

        private void SetupViewForTool(ToolType toolType) {
            foreach (ToolButton toolButton in View.ToolButtons) {
                if(toolType != toolButton.ToolType)
                    continue;
                toolButton.SetupView(GetToolButtonViewData(toolButton.ToolType));
            }
        }

        private ToolButtonViewData GetToolButtonViewData(ToolType toolButtonToolType) {
            return new ToolButtonViewData() {
                HasTool = _toolService.HasTool(toolButtonToolType), 
                Amount = _toolService.GetToolAmount(toolButtonToolType),
                Price = _priceDataProvider.GetPrice(GetTransactionId(toolButtonToolType)),
                Blocked = CheckForLock(toolButtonToolType)
            };
        }

        private bool CheckForLock(ToolType toolType) {
            return !_toolService.CanUseTool(toolType);
        }

        private TransactionId GetTransactionId(ToolType toolType) {
            switch (toolType) {
                case ToolType.Undo:
                    return TransactionId.BuyUndo;
                default:
                    return TransactionId.None;
            }
        }
    }
}