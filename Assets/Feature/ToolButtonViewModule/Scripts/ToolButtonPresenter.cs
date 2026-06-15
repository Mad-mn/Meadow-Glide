using Feature.ToolModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.ToolButtonViewModule.Scripts {
    public class ToolButtonPresenter : PresenterBase<ToolButtonView> {
        private readonly IToolService _toolService;
        public ToolButtonPresenter(ToolButtonView view, IToolService toolService) : base(view) {
            _toolService = toolService;
        }

        public override void Initialize() {
            foreach (ToolButton toolButton in View.ToolButtons) {
                toolButton.OnButtonClick += OnButtonClick;
            }
        }

        public override void Show() {
            base.Show();
            foreach (ToolButton toolButton in View.ToolButtons) {
                toolButton.LockIcon.SetActive(CheckForLock(toolButton.ToolType));
            }
        }

        private void OnButtonClick(ToolType toolType) {
            if (CheckForLock(toolType))
                return;

            _toolService.ExecuteTool(toolType);
        }

        private bool CheckForLock(ToolType toolType) {
            return !_toolService.CanUseTool(toolType);
        }
    }
}