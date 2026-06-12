using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Zenject;

namespace Feature.StripRotationModule.Scripts {
    public class StripRotationService : IStripRotationService, ITickable, IInitializable, IDisposable {
        private readonly IInputService _inputService;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IInteractionStateService _interactionStateService;
        private readonly IAudioService _audioService;
        private readonly IVibrationService _vibrationService;

        private readonly List<StripController> _strips;
        private StripController _activeStrip;
        private bool _isDragging;
        private bool _zomedIn;

        public StripRotationService(IInputService inputService, MoveTrackModel moveTrackModel, IInteractionStateService interactionStateService,
            IAudioService audioService, IVibrationService vibrationService) {
            _inputService = inputService;
            _moveTrackModel = moveTrackModel;
            _interactionStateService = interactionStateService;
            _audioService = audioService;
            _vibrationService = vibrationService;
        }
        public void Register(StripController strip) {
            if(_strips.Contains(strip))
                return;
            _strips.Add(strip);
        }

        public void Clear() {
            _strips.Clear();
        }

        public bool IsInteracting { get; }

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
            if (_interactionStateService.IsSlideActive) {
                return;
            }

            RotateStrip();
        }

        private void OnPointerDown() {
            if(_moveTrackModel.MovesLeft<=0)
                return;
            if (_interactionStateService.IsSlideActive) {
                return;
            }

            TryStartRotation();
        }

        private void OnPointerUp() {
            if (_activeStrip != null) {
                if (_isDragging) {
                    SnapStrip(_activeStrip).Forget();
                }
                ChangeCircleScaleOnRotation(false);
                _activeStrip = null;
            }
            _isDragging = false;
            _interactionStateService.IsRotationActive = false;
        }

        private void RotateStrip() {
            //TODO: MoveStrip left-right by _inputService.PointerPosition, create ghost like in SlideSegment if stripSegment move to\from away
        }

        private async UniTaskVoid SnapStrip(StripController activeStrip) {
            ///TODO: Snap strip to final position calculated by segment wight to nearest position like in CircleRotationService
        }

        private void TryStartRotation() {
            ///TODO: Find _active circle by Input and prepare for rotation
        }

        private void ChangeCircleScaleOnRotation(bool isRotating) {
            if (!_zomedIn && !isRotating)
                return;
            _audioService.PlaySound(isRotating ? AudioType.CircleStartInteraction : AudioType.CircleStopInteraction);
            _vibrationService.PlayVibration(VibrationType.Low);
            foreach (CircleSegment segment in _activeStrip.SpawnedSegments) {
                if(isRotating) {
                    _zomedIn = true;
                    segment.ZoomIn();
                }
                else {
                    _zomedIn = false;
                    segment.ZoomOut();
                }
            }
        }
    }
}