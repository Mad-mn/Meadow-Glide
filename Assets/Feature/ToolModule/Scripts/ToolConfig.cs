using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.ToolModule.Scripts {
    [CreateAssetMenu(fileName = "ToolConfig", menuName = "Configs/Tools/ToolConfig")]
    public class ToolConfig : ScriptableObject {
        [SerializeField] private List<ToolData> _tools;
        
        public IReadOnlyList<ToolData> Tools => _tools;
    }

    [Serializable]
    public class ToolData {
        public ToolType ToolType;
        public int UnlockLevel;
    }
}