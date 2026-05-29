using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using Feature.CircleModule.Scripts;
using Feature.InputModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideSegmentService : ISlideSegmentService, ITickable, IInitializable, IDisposable {
        private readonly IInputService _inputService;
        private readonly ISlideAreaService _slideAreaService;
        private readonly IInteractionStateService _interactionState;
        private readonly ICameraService _cameraService;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private CircleParamsConfig _circleParamsConfig;
        private readonly List<CircleController> _circles = new List<CircleController>();
        
        private Camera _mainCamera;
        private SlideArea _activeArea;
        private float _startRadius;
        private readonly List<CircleSegment> _activeSegments = new List<CircleSegment>();
        private readonly List<float> _baseIndices = new List<float>();
        private readonly List<CircleSegment> _ghosts = new List<CircleSegment>();
        private List<CircleController> _sortedCircles = new List<CircleController>();

        public bool IsSliding => _activeArea != null;

        public SlideSegmentService(
            IInputService inputService, 
            ISlideAreaService slideAreaService,
            IInteractionStateService interactionState,
            ICameraService cameraService,
            UniTask<CircleParamsConfig> circleParamsConfigTask) {
            _inputService = inputService;
            _slideAreaService = slideAreaService;
            _interactionState = interactionState;
            _cameraService = cameraService;
            _circleParamsConfigTask = circleParamsConfigTask;
        }

        public async void Initialize() {
            _inputService.PointerDown += OnPointerDown;
            _inputService.PointerUp += OnPointerUp;
            _circleParamsConfig = await _circleParamsConfigTask;
        }

        public void Dispose() {
            _inputService.PointerDown -= OnPointerDown;
            _inputService.PointerUp -= OnPointerUp;
            ClearGhosts();
        }

        public void RegisterCircle(CircleController circle) {
            _circles.Add(circle);
            _sortedCircles = _circles.OrderBy(c => c.Radius).ToList();
        }

        public void Clear() {
            _circles.Clear();
            _sortedCircles.Clear();
            ClearGhosts();
        }

        private void ClearGhosts() {
            foreach (var ghost in _ghosts) {
                if (ghost != null) UnityEngine.Object.Destroy(ghost.gameObject);
            }
            _ghosts.Clear();
        }

        private void OnPointerDown() {
            if (_interactionState.IsRotationActive) {
                return;
            }

            if (_mainCamera == null) {
                _mainCamera = _cameraService.CameraObject;
            }

            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            worldPos.z = 0;

            _activeArea = FindSlideArea(worldPos);

            if (_activeArea != null) {
                _interactionState.IsSlideActive = true;
                _startRadius = worldPos.magnitude;
                PrepareSegments(_activeArea.SectorIndex);
                _slideAreaService.IsSliding = true;
            }
        }

        private SlideArea FindSlideArea(Vector3 worldPos) {
            foreach (var area in _slideAreaService.SpawnedAreas) {
                var collider = area.GetComponent<PolygonCollider2D>();
                if (collider.OverlapPoint(worldPos)) {
                    return area;
                }
            }
            return null;
        }

        private void PrepareSegments(int sectorIndex) {
            _activeSegments.Clear();
            _baseIndices.Clear();
            ClearGhosts();
            
            for (int i = 0; i < _sortedCircles.Count; i++) {
                var circle = _sortedCircles[i];
                float anglePerSegment = 360f / circle.SegmentCount;
                float worldSectorAngle = sectorIndex * anglePerSegment;
                
                var segment = circle.GetSegmentAtAngle(worldSectorAngle);
                if (segment != null) {
                    _activeSegments.Add(segment);
                    _baseIndices.Add(i);
                    
                    var ghost = UnityEngine.Object.Instantiate(segment, segment.transform.parent);
                    ghost.gameObject.name = segment.gameObject.name + "_Ghost";
                    ghost.SetVisible(false);
                    _ghosts.Add(ghost);
                }
            }
        }

        public void Tick() {
            if (_activeArea == null) return;

            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            worldPos.z = 0;

            float currentRadius = worldPos.magnitude;
            UpdateSegmentsVisuals(currentRadius);
        }

        private void UpdateSegmentsVisuals(float currentRadius) {
            int count = _activeSegments.Count;
            if (count == 0 || _circleParamsConfig == null) return;

            int circleCount = _sortedCircles.Count;
            float startIndex = _circleParamsConfig.GetVirtualIndex(_startRadius);
            float currentIndex = _circleParamsConfig.GetVirtualIndex(currentRadius);
            float deltaIndex = currentIndex - startIndex;

            for (int i = 0; i < count; i++) {
                float virtualIndex = _baseIndices[i] + deltaIndex;
                float wrappedIndex = Mathf.Repeat(virtualIndex, circleCount);
                
                var segment = _activeSegments[i];
                float r = Mathf.Max(0, _circleParamsConfig.GetRadius(wrappedIndex));
                segment.SetRadius(r);
                segment.SetWidth(_circleParamsConfig.GetWidth(wrappedIndex));
                
                float midIndex = circleCount / 2f;
                float ghostIndex = wrappedIndex > midIndex ? wrappedIndex - circleCount : wrappedIndex + circleCount;
                
                var ghost = _ghosts[i];
                float gr = Mathf.Max(0, _circleParamsConfig.GetRadius(ghostIndex));
                ghost.SetRadius(gr);
                ghost.SetWidth(_circleParamsConfig.GetWidth(ghostIndex));
                ghost.SetVisible(true);
                ghost.transform.localRotation = segment.transform.localRotation;
            }
        }

        private void OnPointerUp() {
            if (_activeArea != null) {
                SnapSegments().Forget();
                _activeArea = null;
                _slideAreaService.IsSliding = false;
            }
            _interactionState.IsSlideActive = false;
        }

        private async UniTaskVoid SnapSegments() {
            int count = _activeSegments.Count;
            if (count == 0 || _circleParamsConfig == null) return;

            float firstSegCurrentR = _activeSegments[0].Radius;
            float rawDeltaIndex = _circleParamsConfig.GetVirtualIndex(firstSegCurrentR) - _baseIndices[0];
            
            int circleCount = _sortedCircles.Count;
            float normalizedDeltaIndex = ((rawDeltaIndex % circleCount) + circleCount) % circleCount;
            if (normalizedDeltaIndex > circleCount / 2f) normalizedDeltaIndex -= circleCount;

            int shift = Mathf.RoundToInt(normalizedDeltaIndex);
            
            ApplyShift(shift);

            float duration = 0.2f;
            float elapsed = 0;
            
            float[] startIndices = new float[count];
            float[] targetIndices = new float[count];

            for (int i = 0; i < count; i++) {
                float startIdx = _circleParamsConfig.GetVirtualIndex(_activeSegments[i].Radius);
                float targetIdx = i;

                // Shortest path logic: 
                // If the segment has to travel more than half the circle count, 
                // it's better to wrap it to the other side to avoid "flying" across the whole board.
                if (startIdx - targetIdx > circleCount / 2f) startIdx -= circleCount;
                else if (targetIdx - startIdx > circleCount / 2f) startIdx += circleCount;

                startIndices[i] = startIdx;
                targetIndices[i] = targetIdx;
                _ghosts[i].SetVisible(false);
            }

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3);

                for (int i = 0; i < count; i++) {
                    var seg = _activeSegments[i];
                    if (seg == null) continue;
                    float idx = Mathf.Lerp(startIndices[i], targetIndices[i], t);
                    seg.SetRadius(Mathf.Max(0, _circleParamsConfig.GetRadius(idx)));
                    seg.SetWidth(_circleParamsConfig.GetWidth(idx));
                }
                await UniTask.Yield();
            }

            for (int i = 0; i < count; i++) {
                if (_activeSegments[i] != null) {
                    _activeSegments[i].SetRadius(_circleParamsConfig.GetRadius(i));
                    _activeSegments[i].SetWidth(_circleParamsConfig.GetWidth(i));
                }
            }
            
            ClearGhosts();
            _activeSegments.Clear();
            _baseIndices.Clear();
        }

        private void ApplyShift(int shift) {
            if (shift == 0) return;
            
            int count = _activeSegments.Count;
            var shiftedSegments = new CircleSegment[count];
            for (int i = 0; i < count; i++) {
                int newIndex = ((i + shift) % count + count) % count;
                shiftedSegments[newIndex] = _activeSegments[i];
            }

            for (int i = 0; i < count; i++) {
                var targetCircle = _sortedCircles[i];
                var segment = shiftedSegments[i];
                
                if (segment.transform.parent != null) {
                    var oldCircle = segment.transform.parent.GetComponent<CircleController>();
                    if (oldCircle != null) oldCircle.RemoveSegment(segment);
                }
                
                targetCircle.AddSegment(segment);
                
                float anglePerSegment = 360f / targetCircle.SegmentCount;
                float worldSectorAngle = _activeArea.SectorIndex * anglePerSegment;
                
                float targetLocalAngle = Mathf.DeltaAngle(targetCircle.transform.eulerAngles.z, worldSectorAngle);
                segment.transform.localRotation = Quaternion.Euler(0, 0, targetLocalAngle);
                
                _activeSegments[i] = segment;
            }
        }
    }
}