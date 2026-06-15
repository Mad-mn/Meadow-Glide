using Feature.UndoModule.Scripts;

namespace Feature.ToolModule.Scripts.Tools {
    public class UndoTool : ITool {
        private readonly IUndoService _undoService;

        public UndoTool(IUndoService undoService) {
            _undoService = undoService;
        }
        public void Execute() {
            _undoService.Undo();
        }
    }
}