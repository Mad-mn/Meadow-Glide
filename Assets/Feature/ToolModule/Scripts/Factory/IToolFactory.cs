using Feature.ToolModule.Scripts.Tools;

namespace Feature.ToolModule.Scripts.Factory {
    public interface IToolFactory {
        T CreateTool<T>() where T : ITool;
    }
}