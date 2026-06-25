using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Feature.ToolModule.Scripts {
    public class ToolConfigProvider : IToolConfigProvider {
        private readonly UniTask<ToolConfig> _toolConfigTask;
        private ToolConfig _toolConfig;

        public ToolConfigProvider(UniTask<ToolConfig> toolConfigTask) {
            _toolConfigTask = toolConfigTask;
        }
        
        public async UniTask Initialize() {
            _toolConfig = await _toolConfigTask;
        }

        public IReadOnlyList<ToolData> Tools => _toolConfig.Tools;
    }
}