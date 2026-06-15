using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.AnimationModule.Scripts;
using Feature.CameraServiceModule.Scripts;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using Feature.StripsModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.PreGamePlacementModule.Scripts {
    public class PreGamePlacementService : IPreGamePlacementService, ITickable, IInitializable, IDisposable {
        private const float PoolSpacing = 1.5f;
        private const float PoolBottomMargin = 1.5f;
        private const float ShakeDuration = 0.3f;
        private const float ShakeStrength = 0.05f;
        private const int ShakeVibrato = 15;

        private readonly IInputService _inputService;
        private readonly IInteractionStateService _interactionStateService;
        private readonly ICameraService _cameraService;
        private readonly IStripSpawnService _stripSpawnService;
        private readonly StripModel _stripModel;
        private readonly ICircleColorService _colorService;
        private readonly ISegmentStatusVisualDataProvider _statusVisualDataProvider;
        private readonly IAnimationService _animationService;
        private readonly IInstantiator _instantiator;
        private readonly UniTask<StripController> _stripControllerTask;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;

        private StripController _stripControllerPrefab;
        private CircleParamsConfig _circleParamsConfig;
        private Camera _camera;

        private readonly List<EmptySlot> _emptySlots = new List<EmptySlot>();
        private readonly List<PoolPiece> _poolPieces = new List<PoolPiece>();
        private readonly HashSet<EmptySlot> _highlightedSlots = new HashSet<EmptySlot>();

        private PoolPiece _selectedPiece;
        private bool _isPlacing;
        private bool _isFlying;
        private bool _inputConsumed;
        private int _totalStripCount;
        private Action _onComplete;

        public bool IsActive => _isPlacing;

        public PreGamePlacementService(
            IInputService inputService,
            IInteractionStateService interactionStateService,
            ICameraService cameraService,
            IStripSpawnService stripSpawnService,
            StripModel stripModel,
            ICircleColorService colorService,
            ISegmentStatusVisualDataProvider statusVisualDataProvider,
            IAnimationService animationService,
            IInstantiator instantiator,
            UniTask<StripController> stripControllerTask,
            UniTask<CircleParamsConfig> circleParamsConfigTask) {
            _inputService = inputService;
            _interactionStateService = interactionStateService;
            _cameraService = cameraService;
            _stripSpawnService = stripSpawnService;
            _stripModel = stripModel;
            _colorService = colorService;
            _statusVisualDataProvider = statusVisualDataProvider;
            _animationService = animationService;
            _instantiator = instantiator;
            _stripControllerTask = stripControllerTask;
            _circleParamsConfigTask = circleParamsConfigTask;
        }

        public async void Initialize() {
            _stripControllerPrefab = await _stripControllerTask;
            _circleParamsConfig = await _circleParamsConfigTask;
        }

        public void Dispose() {
            Cancel();
        }

        public bool HasEmptySlots(LevelConfig levelConfig) {
            if (levelConfig?.CircleConfigs == null) return false;

            foreach (var circle in levelConfig.CircleConfigs) {
                if (circle?.Segments == null) continue;
                foreach (var seg in circle.Segments) {
                    if (seg.SegmentStatus == SegmentStatus.Empty)
                        return true;
                }
            }
            return false;
        }

        public async UniTask StartPlacement(LevelConfig levelConfig, int totalStripCount) {
            if (!HasEmptySlots(levelConfig)) return;

            _isPlacing = true;
            _totalStripCount = totalStripCount;
            _interactionStateService.InputBlocked = true;

            CollectEmptySlots(levelConfig);
            InitializeEmptySlotVisuals();
            SpawnPoolPieces();

            await WaitUntilComplete();

            Cleanup();
            _isPlacing = false;
            _interactionStateService.InputBlocked = false;
        }

        public void Cancel() {
            if (!_isPlacing) return;
            Cleanup();
            _isPlacing = false;
            _interactionStateService.InputBlocked = false;
            _onComplete?.Invoke();
        }

        private void CollectEmptySlots(LevelConfig levelConfig) {
            _emptySlots.Clear();

            for (int i = 0; i < levelConfig.CircleConfigs.Count; i++) {
                var circle = levelConfig.CircleConfigs[i];
                if (circle?.Segments == null) continue;

                for (int s = 0; s < circle.Segments.Count; s++) {
                    if (circle.Segments[s].SegmentStatus == SegmentStatus.Empty) {
                        StripController strip = FindStripByIndex(i);
                        if (strip == null) continue;

                        _emptySlots.Add(new EmptySlot {
                            StripIndex = i,
                            SectorIndex = s,
                            Strip = strip,
                            ColorType = circle.Segments[s].ColorType,
                        });
                    }
                }
            }
        }

        private void InitializeEmptySlotVisuals() {
            foreach (var slot in _emptySlots) {
                if (slot.Strip == null) continue;

                if (slot.Strip.SpawnedSegments.Count > slot.SectorIndex) {
                    slot.Segment = slot.Strip.SpawnedSegments[slot.SectorIndex];
                    if (slot.Segment != null) {
                        slot.Segment.SetConfig(new SegmentConfig {
                            ColorType = CircleColorType.None,
                            SegmentStatus = SegmentStatus.Empty,
                            Radius = 0f,
                            Angle = 0f
                        });
                        slot.Segment.SetColor(new Color(0.5f, 0.5f, 0.5f, 0.6f));
                        slot.Segment.SetWidth(_circleParamsConfig.GetUniformSegmentThickness());
                    }
                }
            }
        }

        private void SpawnPoolPieces() {
            _poolPieces.Clear();

            var neededColors = _emptySlots
                .Where(s => s.ColorType != CircleColorType.None)
                .GroupBy(s => s.ColorType)
                .ToDictionary(g => g.Key, g => g.Count());

            var availableColors = new List<CircleColorType>();
            foreach (var kvp in neededColors) {
                for (int i = 0; i < kvp.Value; i++)
                    availableColors.Add(kvp.Key);
            }

            if (availableColors.Count == 0) {
                MarkComplete();
                return;
            }

            float lowestStripeY = GetLowestStripeY();
            float poolY = lowestStripeY - _circleParamsConfig.StripHeight - PoolBottomMargin;

            float totalWidth = (availableColors.Count - 1) * PoolSpacing;
            float startX = -totalWidth * 0.5f;

            int segmentsPerStripe = _emptySlots.Count > 0 ? _emptySlots[0].Strip.SegmentCount : 1;
            float desiredSpan = _circleParamsConfig.StripLoopLength / segmentsPerStripe;

            for (int i = 0; i < availableColors.Count; i++) {
                Vector3 pos = new Vector3(startX + i * PoolSpacing, poolY, 0);

                StripController poolStrip = _instantiator.InstantiatePrefabForComponent<StripController>(_stripControllerPrefab);
                poolStrip.gameObject.name = $"PoolPiece_{availableColors[i]}";

                var dummyConfig = ScriptableObject.CreateInstance<CircleConfig>();
                dummyConfig.SegmentCount = 1;
                dummyConfig.Segments = new List<SegmentConfig> {
                    new SegmentConfig {
                        ColorType = availableColors[i],
                        SegmentStatus = SegmentStatus.Default,
                        Radius = 0f,
                        Angle = 0f
                    }
                };

                poolStrip.Setup(dummyConfig, _circleParamsConfig.GetUniformSegmentThickness(),
                    desiredSpan, 0f, -1);

                poolStrip.transform.position = pos;

                _poolPieces.Add(new PoolPiece {
                    Strip = poolStrip,
                    ColorType = availableColors[i],
                    OriginalPosition = pos,
                });
            }
        }

        private float GetLowestStripeY() {
            float lowestY = float.MaxValue;
            foreach (var strip in _stripModel.Strips) {
                if (strip.CenterY < lowestY)
                    lowestY = strip.CenterY;
            }
            return lowestY;
        }

        private void SetSlotHighlight(EmptySlot slot, bool highlight) {
            if (slot.Segment == null) return;

            if (highlight && !_highlightedSlots.Contains(slot)) {
                _highlightedSlots.Add(slot);
                slot.Segment.ZoomIn(true);
            }
            else if (!highlight && _highlightedSlots.Contains(slot)) {
                _highlightedSlots.Remove(slot);
                slot.Segment.ZoomOut();
            }
        }

        private void HighlightAllEmptySlots(bool highlight) {
            foreach (var slot in _emptySlots) {
                SetSlotHighlight(slot, highlight);
            }
        }

        private void SelectPiece(PoolPiece piece) {
            if (_isFlying) return;

            DeselectPiece();
            _selectedPiece = piece;
            foreach (var segment in piece.Strip.SpawnedSegments) {
                segment.ZoomIn(true);
            }
            HighlightAllEmptySlots(true);
        }

        private void DeselectPiece() {
            if (_selectedPiece == null) return;

            foreach (var segment in _selectedPiece.Strip.SpawnedSegments) {
                segment.ZoomOut();
            }
            _selectedPiece = null;
            HighlightAllEmptySlots(false);
        }

        private async UniTask PlacePiece(EmptySlot slot) {
            if (_selectedPiece == null || _isFlying) return;

            _isFlying = true;
            PoolPiece piece = _selectedPiece;
            _selectedPiece = null;

            StripSegment targetSegment = slot.Segment;
            if (targetSegment == null) {
                _isFlying = false;
                return;
            }

            HighlightAllEmptySlots(false);

            Vector3 endPos = targetSegment.transform.position;
            var flyTcs = new UniTaskCompletionSource();
            _animationService.PlayFly(piece.Strip.transform, endPos, () => flyTcs.TrySetResult());
            await flyTcs.Task;

            SegmentConfig placedConfig = new SegmentConfig {
                ColorType = piece.ColorType,
                SegmentStatus = SegmentStatus.Default,
                Radius = 0f,
                Angle = 0f
            };

            targetSegment.ForceResetZoom();
            targetSegment.SetConfig(placedConfig);
            Color color = _colorService.GetColor(piece.ColorType);
            targetSegment.SetColor(color);
            targetSegment.SetWidth(_circleParamsConfig.GetUniformSegmentThickness());
            targetSegment.SetVisible(false);

            var landTcs = new UniTaskCompletionSource();
            _animationService.PlayLand(piece.Strip.transform, () => landTcs.TrySetResult());
            await landTcs.Task;
            targetSegment.SetVisible(true);

            _emptySlots.Remove(slot);
            _highlightedSlots.Remove(slot);
            UnityEngine.Object.Destroy(piece.Strip.gameObject);
            _poolPieces.Remove(piece);

            _stripModel.SegmentsChanged();

            _isFlying = false;

            if (_emptySlots.Count == 0)
                MarkComplete();
        }

        private void MarkComplete() {
            HighlightAllEmptySlots(false);
            _onComplete?.Invoke();
        }

        private void Cleanup() {
            DeselectPiece();

            foreach (var piece in _poolPieces) {
                if (piece.Strip != null)
                    UnityEngine.Object.Destroy(piece.Strip.gameObject);
            }
            _poolPieces.Clear();

            foreach (var slot in _emptySlots) {
                SetSlotHighlight(slot, false);
            }
            _emptySlots.Clear();
            _highlightedSlots.Clear();
        }

        private StripController FindStripByIndex(int index) {
            foreach (var strip in _stripModel.Strips) {
                if (strip.PositionIndex == index)
                    return strip;
            }
            return null;
        }

        private UniTask WaitUntilComplete() {
            var tcs = new UniTaskCompletionSource();
            _onComplete = () => tcs.TrySetResult();
            return tcs.Task;
        }

        public void Tick() {
            if (!_isPlacing || _isFlying) return;

            bool pointerDown = _inputService.IsPointerPressed;
            if (pointerDown && !_inputConsumed) {
                _inputConsumed = true;
                HandlePlacementInput();
            }
            else if (!pointerDown) {
                _inputConsumed = false;
            }
        }

        private void HandlePlacementInput() {
            if (_camera == null)
                _camera = _cameraService.CameraObject;
            if (_camera == null) return;

            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_camera.transform.position.z));
            worldPos.z = 0;

            if (_selectedPiece != null) {
                foreach (var slot in _emptySlots) {
                    if (slot.Segment == null) continue;
                    float dist = Vector2.Distance(worldPos, slot.Segment.transform.position);
                    if (dist < 0.5f) {
                        PlacePiece(slot).Forget();
                        return;
                    }
                }

                DeselectPiece();
                ShakePoolPieces();
            }
            else {
                foreach (var piece in _poolPieces) {
                    float dist = Vector2.Distance(worldPos, piece.Strip.transform.position);
                    if (dist < 0.8f) {
                        SelectPiece(piece);
                        return;
                    }
                }

                ShakePoolPieces();
            }
        }

        private void ShakePoolPieces() {
            var transforms = _poolPieces
                .Where(p => p.Strip != null)
                .Select(p => p.Strip.transform);
            _animationService.PlayShake(transforms, ShakeDuration, ShakeStrength, ShakeVibrato);
        }

        private class EmptySlot {
            public int StripIndex;
            public int SectorIndex;
            public StripController Strip;
            public CircleColorType ColorType;
            public StripSegment Segment;
        }

        private class PoolPiece {
            public StripController Strip;
            public CircleColorType ColorType;
            public Vector3 OriginalPosition;
        }
    }
}
