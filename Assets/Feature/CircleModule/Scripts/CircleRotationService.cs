using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using Feature.TrackMoveModule.Scripts;
using UnityEngine;
using Zenject;
using AudioType = Feature.SoundModule.Scripts.AudioType;

namespace Feature.CircleModule.Scripts {
    public class CircleRotationService : ICircleRotationService, ITickable, IInitializable, IDisposable {
        private const float RADIUS_THRESHOLD = 0.2f;
        private const float ROTATION_ANGLE_THRESHOLD = 2.0f; // Threshold in degrees
        
        private readonly IInputService _inputService;
        private readonly IInteractionStateService _interactionState;
        private readonly GameCircleModel _circleModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ISlideSegmentService _slideSegmentService;
        private readonly ICameraService _cameraService;
        private readonly IAudioService _audioService;

        private readonly List<CircleController> _circles = new List<CircleController>();

        private CircleController _activeCircle;
        private float _startAngle;
        private float _initialCircleRotation;

        private bool _isDragging;

        public bool IsInteracting => _activeCircle != null && _isDragging;

        public CircleRotationService(IInputService inputService, IInteractionStateService interactionState,
            GameCircleModel circleModel, MoveTrackModel moveTrackModel, ISlideSegmentService slideSegmentService,
            ICameraService cameraService, IAudioService audioService) {
            _inputService = inputService;
            _interactionState = interactionState;
            _circleModel = circleModel;
            _moveTrackModel = moveTrackModel;
            _slideSegmentService = slideSegmentService;
            _cameraService = cameraService;
            _audioService = audioService;
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
            if(_moveTrackModel.MovesLeft<=0)
                return;
            if (_interactionState.IsSlideActive) {
                return;
            }

            TryStartRotation();
        }

        private void TryStartRotation() {
            Vector2 screenPos = _inputService.PointerPosition;
            var camera = _cameraService.CameraObject;
            
            if (camera == null) {
                return;
            }
            
            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0;
            
            float distToCenter = worldPos.magnitude;

            _activeCircle = _circles
                .OrderBy(c => Mathf.Abs(c.Radius - distToCenter))
                .FirstOrDefault(c => Mathf.Abs(c.Radius - distToCenter) < RADIUS_THRESHOLD);

            if (_activeCircle != null) {
                _startAngle = Mathf.Atan2(worldPos.y, worldPos.x) * Mathf.Rad2Deg;
                _initialCircleRotation = _activeCircle.transform.eulerAngles.z;
                _isDragging = false;
                _circleModel.CircleRotationStatusChanges(_activeCircle, true);
                TryChangeScaleCircleWithDelay().Forget();
            }
        }

        private void OnPointerUp() {
            if (_activeCircle != null) {
                if (_isDragging) {
                    SnapCircle(_activeCircle).Forget();
                }
                ChangeCircleScaleOnRotation(false);
                _activeCircle = null;
            }
            _isDragging = false;
            _interactionState.IsRotationActive = false;
        }

        public void Tick() {
            if (_activeCircle == null) return;
            if (_interactionState.IsSlideActive) {
                return;
            }

            RotateCircle();
        }

        private void RotateCircle() {
            Vector2 screenPos = _inputService.PointerPosition;
            var camera = Camera.main ?? GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
            if (camera == null) return;

            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0;

            float currentAngle = Mathf.Atan2(worldPos.y, worldPos.x) * Mathf.Rad2Deg;
            float totalDelta = Mathf.Abs(Mathf.DeltaAngle(_startAngle, currentAngle));

            if (!_isDragging) {
                if (totalDelta > ROTATION_ANGLE_THRESHOLD) {
                    _isDragging = true;
                    _interactionState.IsRotationActive = true;
                }

                return;
            }
            
            float angleDelta = Mathf.DeltaAngle(_startAngle, currentAngle);
            _activeCircle.transform.rotation = Quaternion.Euler(0, 0, _initialCircleRotation + angleDelta);
        }

        private async UniTaskVoid TryChangeScaleCircleWithDelay() {
            await UniTask.Yield();
            if (_interactionState.IsSlideActive) {
                return;
            }
            ChangeCircleScaleOnRotation(true);
        }

        private void ChangeCircleScaleOnRotation(bool isRotating) {
            _audioService.PlaySound(isRotating ? AudioType.CircleStartInteraction : AudioType.CircleStopInteraction);
            foreach (CircleSegment segment in _activeCircle.SpawnedSegments) {
                if(isRotating)
                    segment.ZoomIn();
                else {
                    segment.ZoomOut();
                }
            }
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
            
            _slideSegmentService.UpdateSegmentsInAreas();
            _circleModel.CircleRotationStatusChanges(circle, false);
        }
    }
}