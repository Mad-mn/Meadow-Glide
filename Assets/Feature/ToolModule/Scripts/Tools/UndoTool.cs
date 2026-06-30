using Feature.AnalyticsModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.UndoModule.Scripts;

namespace Feature.ToolModule.Scripts.Tools {
    public class UndoTool : ITool {
        private readonly IUndoService _undoService;
        private readonly IAnalyticsService _analyticsService;
        private readonly LevelModel _levelModel;
        private readonly ISaveDataModel _saveDataModel;

        public UndoTool(IUndoService undoService, IAnalyticsService analyticsService,
            LevelModel levelModel, ISaveDataModel saveDataModel) {
            _undoService = undoService;
            _analyticsService = analyticsService;
            _levelModel = levelModel;
            _saveDataModel = saveDataModel;
        }
        public void Execute() {
            _undoService.Undo();
            SendAnalytics();
        }

        private void SendAnalytics() {
            int levelId = _levelModel.ReplayLevel ?? _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level;
            _analyticsService.UndoMoveUsed(levelId);
        }
    }
}