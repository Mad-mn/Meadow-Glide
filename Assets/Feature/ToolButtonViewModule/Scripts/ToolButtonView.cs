using System.Collections.Generic;
using Feature.ToolModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Feature.ToolButtonViewModule.Scripts {
    public class ToolButtonView : ViewBase {
        [SerializeField] private List<ToolButton> _toolButtons;
        
        public IReadOnlyList<ToolButton> ToolButtons => _toolButtons;
    }
}