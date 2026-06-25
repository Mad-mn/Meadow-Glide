using System.Collections.Generic;

namespace Feature.ToolButtonViewModule.Scripts {
    public class ToolButtonsViewModel {
        public IReadOnlyList<ToolButton> ToolButtons { get; private set; }
        
        public void SetToolButtons(IReadOnlyList<ToolButton> toolButtons) {
            ToolButtons = toolButtons;
        }
    }
}