
using Cysharp.Threading.Tasks;

namespace Feature.FirebaseModule.Scripts {
    public interface IFirebaseService {
        bool IsInitialized { get; }
        UniTask Initialize();
    }
}
