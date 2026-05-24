using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Feature.ColorServiceModule.Scripts {
    public class CircleColorService : ICircleColorService, IInitializable {
        private readonly UniTask<CircleColorProvider> _circleColorProviderTask;
        private CircleColorProvider _circleColorProvider;
        private bool _initialized;

        public CircleColorService(UniTask<CircleColorProvider> circleColorProvider) {
            _circleColorProviderTask = circleColorProvider;
        }

        public Color GetColor(CircleColorType type)
        {
            if (!_initialized) {
                Debug.LogError("Service not initialized");
                return Color.white;
            }
            var mapping = _circleColorProvider.Mappings.FirstOrDefault(map => map.Type == type);
            return mapping.Type != CircleColorType.None ? mapping.Color : Color.white;
        }

        public void Initialize() {
            InitializeAsync().Forget();
        }

        private async UniTaskVoid InitializeAsync() {
            _circleColorProvider = await _circleColorProviderTask;
            _initialized = true;
        }
    }
}