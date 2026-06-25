using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;

namespace Feature.StripsModule.Scripts {
    public interface IStripSpawnService {
        UniTask Initialize();
        StripController SpawnStrip(CircleConfig config, int positionIndex, int totalStripCount);
    }
}
