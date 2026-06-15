using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.ToolModule.Scripts.Factory;
using Feature.ToolModule.Scripts.Tools;
using UnityEngine;

namespace Feature.ToolModule.Scripts {
    public class ToolService : IToolService {
        private readonly IToolConfigProvider _toolConfigProvider;
        private readonly SaveDataModel _saveData;
        private readonly IToolFactory _toolFactory;

        private readonly Dictionary<ToolType, ITool> _tools = new();
        
        public ToolService(IToolConfigProvider toolConfigProvider, SaveDataModel saveData, IToolFactory toolFactory) {
            _toolConfigProvider = toolConfigProvider;
            _saveData = saveData;
            _toolFactory = toolFactory;
        }
        public void ExecuteTool(ToolType toolType) {
            if(!CanUseTool(toolType))
                return;

            ITool tool = GetTool(toolType);
            tool?.Execute();
        }

        private ITool GetTool(ToolType toolType) {
            if (!_tools.TryGetValue(toolType, out ITool tool)) {
                return CreateToolByType(toolType);
            }
            
            return tool;
        }

        private ITool CreateToolByType(ToolType toolType) {
            switch (toolType) {
                case ToolType.Undo:
                    ITool tool = _toolFactory.CreateTool<UndoTool>();
                    _tools.Add(toolType, tool);
                    return tool;
                default:
                    return null;
            }
        }

        public bool CanUseTool(ToolType toolType) {
            return _saveData.Get<PlayerProgressData>(SaveDataType.PlayerProgress)
                .Level >= GetToolData(toolType)
                .UnlockLevel;
        }

        public bool HasTool(ToolType toolType) =>
            throw new System.NotImplementedException();

        private ToolData GetToolData(ToolType toolType) {
            ToolData data = _toolConfigProvider.Tools.FirstOrDefault(data => data.ToolType == toolType);

            if (data == null) {
                Debug.LogError($"Tool data not found for type: {toolType}");
                return null;
            }

            return data;
        }
    }
}