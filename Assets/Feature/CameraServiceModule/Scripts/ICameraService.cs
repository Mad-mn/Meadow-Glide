using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Feature.CameraServiceModule.Scripts {
    public interface ICameraService {
        Camera CameraObject { get; }
        UniTask Initialize();
    }
}