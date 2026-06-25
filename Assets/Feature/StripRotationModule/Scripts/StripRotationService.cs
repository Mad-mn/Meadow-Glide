using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using Feature.CircleModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.UndoModule.Scripts;
using Feature.UndoModule.Scripts.Actions;
using UnityEngine;
using Zenject;
using AudioType = Feature.SoundModule.Scripts.AudioType;

namespace Feature.StripRotationModule.Scripts {
    public class StripRotationService : IStripRotationService, ITickable, IInitializable, System.IDisposable {
        private const float Y_THRESHOLD = 0.25f;
        private const float DRAG_THRESHOLD = 0.05f;

        private readonly IInputService _inputService;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IInteractionStateService _interactionStateService;
        private readonly IAudioService _audioService;
        private readonly IVibrationService _vibrationService;
        private readonly StripModel _stripModel;
        private readonly ISlideSegmentService _slideSegmentService;
        private readonly ICameraService _cameraService;
        private readonly IUndoService _undoService;

        private readonly List<StripController> _strips = new List<StripController>();

        private StripController _activeStrip;
        private float _startPointerX;
        private float _initialScrollOffset;
        private bool _isDragging;
        private bool _zoomedIn;

        public bool IsInteracting => _activeStrip != null && _isDragging;

        public StripRotationService(IInputService inputService, MoveTrackModel moveTrackModel, IInteractionStateService interactionStateService,
            IAudioService audioService, IVibrationService vibrationService, StripModel stripModel, ISlideSegmentService slideSegmentService,
            ICameraService cameraService, IUndoService undoService) {
            _inputService = inputService;
            _moveTrackModel = moveTrackModel;
            _interactionStateService = interactionStateService;
            _audioService = audioService;
            _vibrationService = vibrationService;
            _stripModel = stripModel;
            _slideSegmentService = slideSegmentService;
            _cameraService = cameraService;
            _undoService = undoService;
        }

        public void Register(StripController strip) {
            if (_strips.Contains(strip))
                return;
            _strips.Add(strip);
        }

        public void Clear() {
            _strips.Clear();
        }

        public void Initialize() {
            _inputService.PointerDown += OnPointerDown;
            _inputService.PointerUp += OnPointerUp;
        }

        public void Dispose() {
            _inputService.PointerDown -= OnPointerDown;
            _inputService.PointerUp -= OnPointerUp;
        }

        public void Tick() {
            if (_activeStrip == null) return;
            if (_interactionStateService.IsSlideActive)
                return;

            MoveStrip();
        }

        private void OnPointerDown() {
            if (_moveTrackModel.MovesLeft <= 0)
                return;
            if (_interactionStateService.IsSlideActive || _interactionStateService.InputBlocked)
                return;

            TryStartRotation();
        }

        private void OnPointerUp() {
            if (_activeStrip != null) {
                if (_isDragging) {
                    SnapStrip(_activeStrip).Forget();
                }
                else {
                    _interactionStateService.IsRotationActive = false;
                }

                ChangeStripScaleOnRotation(false);
                _activeStrip.ClearWrapGhosts();
                _activeStrip = null;
            }

            _isDragging = false;
        }

        private void TryStartRotation() {
            Camera camera = _cameraService.CameraObject;
            if (camera == null)
                return;

            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0f;

            _activeStrip = _strips
                .OrderBy(strip => Mathf.Abs(strip.CenterY - worldPos.y))
                .FirstOrDefault(strip => Mathf.Abs(strip.CenterY - worldPos.y) < Y_THRESHOLD);

            if (_activeStrip == null)
                return;

            _startPointerX = worldPos.x;
            _initialScrollOffset = _activeStrip.ScrollOffset;
            _isDragging = false;
            _stripModel.CircleRotationStatusChanges(_activeStrip, true);
            TryChangeScaleWithDelay().Forget();
        }

        private void MoveStrip() {
            Camera camera = _cameraService.CameraObject ?? Camera.main;
            if (camera == null)
                return;

            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0f;

            float deltaX = worldPos.x - _startPointerX;
            if (!_isDragging) {
                if (Mathf.Abs(deltaX) > DRAG_THRESHOLD) {
                    _isDragging = true;
                    _interactionStateService.IsRotationActive = true;
                }
                else {
                    return;
                }
            }

            float scrollOffset = _initialScrollOffset - deltaX;
            _activeStrip.SetScrollOffset(scrollOffset, true);
        }

        private async UniTaskVoid SnapStrip(StripController activeStrip) {
            if (activeStrip.SegmentCount <= 0) {
                _interactionStateService.IsRotationActive = false;
                return;
            }

            float segmentSpan = activeStrip.GetSegmentSpan();
            float currentOffset = activeStrip.ScrollOffset;
            float targetOffset = Mathf.Round(currentOffset / segmentSpan) * segmentSpan;

            float startOffset = currentOffset;
            const float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration) {
                if (activeStrip == null) {
                    _interactionStateService.IsRotationActive = false;
                    return;
                }

                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3f);
                float offset = Mathf.Lerp(startOffset, targetOffset, t);
                activeStrip.SetScrollOffset(offset, true);
                await UniTask.Yield();
            }

            if (activeStrip != null) {
                activeStrip.SetScrollOffset(targetOffset, false);
                activeStrip.ClearWrapGhosts();
            }

            _slideSegmentService.UpdateSegmentsInAreas();
            _stripModel.CircleRotationStatusChanges(activeStrip, false);
            _interactionStateService.IsRotationActive = false;

            if (Mathf.Abs(targetOffset - _initialScrollOffset) > 0.01f) {
                var action = new RotationUndoAction(
                    activeStrip,
                    _initialScrollOffset,
                    targetOffset,
                    true,
                    _moveTrackModel,
                    _slideSegmentService,
                    _stripModel
                );
                _undoService.Record(action);
            }
        }

        private async UniTaskVoid TryChangeScaleWithDelay() {
            await UniTask.Yield();
            if (_interactionStateService.IsSlideActive)
                return;

            ChangeStripScaleOnRotation(true);
        }

        private void ChangeStripScaleOnRotation(bool isRotating) {
            if (!_zoomedIn && !isRotating)
                return;

            _audioService.PlaySound(isRotating ? AudioType.CircleStartInteraction : AudioType.CircleStopInteraction);
            _vibrationService.PlayVibration(VibrationType.Low);

            foreach (StripSegment segment in _activeStrip.SpawnedSegments) {
                if (isRotating) {
                    _zoomedIn = true;
                    segment.ZoomIn();
                }
                else {
                    _zoomedIn = false;
                    segment.ZoomOut();
                }
            }
        }
    }
}
