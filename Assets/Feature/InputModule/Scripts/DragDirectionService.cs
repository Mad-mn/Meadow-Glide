using Feature.CameraServiceModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.InputModule.Scripts {
    public class DragDirectionService : IDragDirectionService, ITickable {
        private const float DRAG_THRESHOLD = 0.05f;

        private readonly IInputService _inputService;
        private readonly ICameraService _cameraService;
        private readonly DragDirectionModel _model;

        private Vector2 _startWorldPos;
        private bool _isTracking;

        public DragDirectionService(IInputService inputService, ICameraService cameraService, DragDirectionModel model) {
            _inputService = inputService;
            _cameraService = cameraService;
            _model = model;
        }

        public void Reset() {
            _isTracking = false;
        }

        public void StartTracking(Vector2 startWorldPos) {
            _startWorldPos = startWorldPos;
            _isTracking = true;
        }

        public void Tick() {
            if (!_inputService.IsPointerPressed || !_isTracking)
                return;

            Vector3 worldPos = GetWorldPosition();
            float deltaX = Mathf.Abs(worldPos.x - _startWorldPos.x);
            float deltaY = Mathf.Abs(worldPos.y - _startWorldPos.y);

            if (deltaX > DRAG_THRESHOLD || deltaY > DRAG_THRESHOLD) {
                var direction = deltaX > deltaY ? DragDirection.Horizontal : DragDirection.Vertical;
                _model.DetectDirection(direction);
                _isTracking = false;
            }
        }

        private Vector3 GetWorldPosition() {
            var camera = _cameraService.CameraObject ?? Camera.main;
            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0;
            return worldPos;
        }
    }
}
