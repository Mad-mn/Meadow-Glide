using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Feature.CameraServiceModule.Scripts {
    public class CameraService : ICameraService {
        private readonly DiContainer _container;
        private readonly UniTask<Camera> _cameraTask;
        public Camera CameraObject { get; private set; }

        public CameraService(DiContainer container, UniTask<Camera> cameraTask) {
            _container = container;
            _cameraTask = cameraTask;
        }
        public async UniTask Initialize() {
            Camera prefab = await _cameraTask;
            CameraObject = _container.InstantiatePrefabForComponent<Camera>(prefab);
            GameObject.DontDestroyOnLoad(CameraObject);
        }
    }
}