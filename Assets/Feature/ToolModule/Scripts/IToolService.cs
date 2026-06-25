using Cysharp.Threading.Tasks;

namespace Feature.ToolModule.Scripts {
    public interface IToolService {
        void ExecuteTool(ToolType toolType);
        bool CanUseTool(ToolType toolType);
        bool HasTool(ToolType toolType);
        int GetToolAmount(ToolType toolType);
    }
}