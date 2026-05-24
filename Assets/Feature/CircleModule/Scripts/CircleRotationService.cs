using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.InputModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleRotationService : ICircleRotationService, ITickable, IInitializable, IDisposable {
        private readonly IInputService _inputService;
        private readonly List<CircleController> _circles = new List<CircleController>();
        
        private CircleController _activeCircle;
        private float _startAngle;
        private float _initialCircleRotation;
        private const float RadiusThreshold = 0.2f;

        public CircleRotationService(IInputService inputService) {
            _inputService = inputService;
        }

        public void Initialize() {
            _inputService.PointerDown += OnPointerDown;
            _inputService.PointerUp += OnPointerUp;
        }

        public void Dispose() {
            _inputService.PointerDown -= OnPointerDown;
            _inputService.PointerUp -= OnPointerUp;
        }

        public void Register(CircleController circle) {
            _circles.Add(circle);
        }

        public void Clear() {
            _circles.Clear();
        }

        private void OnPointerDown() {
            Vector2 screenPos = _inputService.PointerPosition;
            var camera = Camera.main;
            
            if (camera == null) {
                camera = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
            }

            if (camera == null) return;
            
            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0;
            
            float distToCenter = worldPos.magnitude;
            
            _activeCircle = _circles
                .OrderBy(c => Mathf.Abs(c.Radius - distToCenter))
                .FirstOrDefault(c => Mathf.Abs(c.Radius - distToCenter) < RadiusThreshold);

            if (_activeCircle != null) {
                _startAngle = Mathf.Atan2(worldPos.y, worldPos.x) * Mathf.Rad2Deg;
                _initialCircleRotation = _activeCircle.transform.eulerAngles.z;
            }
        }

        private void OnPointerUp() {
            if (_activeCircle != null) {
                SnapCircle(_activeCircle).Forget();
                _activeCircle = null;
            }
        }

        public void Tick() {
            if (_activeCircle == null) return;

            Vector2 screenPos = _inputService.PointerPosition;
            var camera = Camera.main;
            if (camera == null) {
                camera = GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
            }
            
            if (camera == null) return;

            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0;

            float currentAngle = Mathf.Atan2(worldPos.y, worldPos.x) * Mathf.Rad2Deg;
            float angleDelta = currentAngle - _startAngle;
            
            _activeCircle.transform.rotation = Quaternion.Euler(0, 0, _initialCircleRotation + angleDelta);
        }

        private async UniTaskVoid SnapCircle(CircleController circle) {
            if (circle.SegmentCount <= 0) return;
            
            float angleStep = 360f / circle.SegmentCount;
            float currentRot = circle.transform.eulerAngles.z;
            
            currentRot = (currentRot % 360 + 360) % 360;
            
            float targetRot = Mathf.Round(currentRot / angleStep) * angleStep;

            float startRot = currentRot;
            float duration = 0.25f;
            float elapsed = 0;

            while (elapsed < duration) {
                if (circle == null) return;
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3); // Ease out cubic
                float rot = Mathf.LerpAngle(startRot, targetRot, t);
                circle.transform.rotation = Quaternion.Euler(0, 0, rot);
                await UniTask.Yield();
            }

            if (circle != null) {
                circle.transform.rotation = Quaternion.Euler(0, 0, targetRot);
            }
        }
    }
}