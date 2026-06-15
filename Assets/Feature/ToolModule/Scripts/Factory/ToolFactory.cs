using Feature.ToolModule.Scripts.Tools;
using Zenject;

namespace Feature.ToolModule.Scripts.Factory {
    public class ToolFactory : IToolFactory {
        private readonly IInstantiator _instantiator;

        public ToolFactory(IInstantiator instantiator) {
            _instantiator = instantiator;
        }
        
        public T CreateTool<T>() where T : ITool {
            return _instantiator.Instantiate<T>();
        }
    }
}