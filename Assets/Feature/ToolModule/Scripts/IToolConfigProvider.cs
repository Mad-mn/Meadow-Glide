using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Feature.ToolModule.Scripts {
    public interface IToolConfigProvider {
        UniTask Initialize();
        IReadOnlyList<ToolData> Tools { get; }
    }
}