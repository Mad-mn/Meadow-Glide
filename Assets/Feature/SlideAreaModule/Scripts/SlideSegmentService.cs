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
        private readonly List<CircleController> _circles = new List<CircleController>();
        
        private Camera _mainCamera;
        private SlideArea _activeArea;
        private float _startRadius;
        private readonly List<CircleSegment> _activeSegments = new List<CircleSegment>();
        private readonly List<float> _baseRadii = new List<float>();
        private readonly List<CircleSegment> _ghosts = new List<CircleSegment>();
        private List<CircleController> _sortedCircles = new List<CircleController>();

        private float _minR, _maxR, _step, _totalSpan;

        public bool IsSliding => _activeArea != null;

        public SlideSegmentService(
            IInputService inputService, 
            ISlideAreaService slideAreaService,
            IInteractionStateService interactionState,
            ICameraService cameraService) {
            _inputService = inputService;
            _slideAreaService = slideAreaService;
            _interactionState = interactionState;
            _cameraService = cameraService;
        }

        public void Initialize() {
            _inputService.PointerDown += OnPointerDown;
            _inputService.PointerUp += OnPointerUp;
            _mainCamera = _cameraService.CameraObject;
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
                _mainCamera = Camera.main;
            }
            if (_mainCamera == null) {
                Debug.LogError(1);
                _mainCamera = Camera.main ?? GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
            }

            if (_mainCamera == null) {
                Debug.LogError(2);
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
            _baseRadii.Clear();
            ClearGhosts();
            
            foreach (var circle in _sortedCircles) {
                float anglePerSegment = 360f / circle.SegmentCount;
                float worldSectorAngle = sectorIndex * anglePerSegment;
                
                var segment = circle.GetSegmentAtAngle(worldSectorAngle);
                if (segment != null) {
                    _activeSegments.Add(segment);
                    _baseRadii.Add(circle.Radius);
                    
                    var ghost = UnityEngine.Object.Instantiate(segment, segment.transform.parent);
                    ghost.gameObject.name = segment.gameObject.name + "_Ghost";
                    ghost.SetVisible(false);
                    _ghosts.Add(ghost);
                }
            }

            if (_baseRadii.Count > 0) {
                _minR = _baseRadii[0];
                _maxR = _baseRadii[^1]; // Assuming sorted
                _step = _baseRadii.Count > 1 ? (_baseRadii[1] - _baseRadii[0]) : 1.0f;
                _totalSpan = _maxR - _minR + _step;
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
            float delta = currentRadius - _startRadius;

            UpdateSegmentsVisuals(delta);
        }

        private void UpdateSegmentsVisuals(float delta) {
            int count = _activeSegments.Count;
            if (count == 0) return;

            float midR = (_minR + _maxR) / 2f;

            for (int i = 0; i < count; i++) {
                float virtualR = _baseRadii[i] + delta;
                float wrappedR = _minR + Mathf.Repeat(virtualR - _minR, _totalSpan);
                
                var segment = _activeSegments[i];
                segment.SetRadius(wrappedR);
                
                float ghostR = wrappedR > midR ? wrappedR - _totalSpan : wrappedR + _totalSpan;
                
                var ghost = _ghosts[i];
                ghost.SetRadius(ghostR);
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
            if (count == 0) return;

            float firstSegCurrentR = _activeSegments[0].Radius;
            float rawDelta = firstSegCurrentR - _baseRadii[0];
            
            float normalizedDelta = ((rawDelta % _totalSpan) + _totalSpan) % _totalSpan;
            if (normalizedDelta > _totalSpan / 2f) normalizedDelta -= _totalSpan;

            int shift = Mathf.RoundToInt(normalizedDelta / _step);
            
            ApplyShift(shift);

            float duration = 0.2f;
            float elapsed = 0;
            
            float[] startRadii = new float[count];
            float[] targetRadii = new float[count];

            for (int i = 0; i < count; i++) {
                startRadii[i] = _activeSegments[i].Radius;
                targetRadii[i] = _sortedCircles[i].Radius;
                _ghosts[i].SetVisible(false);
            }

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3);

                for (int i = 0; i < count; i++) {
                    var seg = _activeSegments[i];
                    if (seg == null) continue;
                    float r = Mathf.Lerp(startRadii[i], targetRadii[i], t);
                    seg.SetRadius(r);
                }
                await UniTask.Yield();
            }

            for (int i = 0; i < count; i++) {
                if (_activeSegments[i] != null)
                    _activeSegments[i].SetRadius(targetRadii[i]);
            }
            
            ClearGhosts();
            _activeSegments.Clear();
            _baseRadii.Clear();
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
                
                // Optimized removal: segment knows its parent
                if (segment.transform.parent != null) {
                    var oldCircle = segment.transform.parent.GetComponent<CircleController>();
                    if (oldCircle != null) oldCircle.RemoveSegment(segment);
                }
                
                targetCircle.AddSegment(segment);
                
                float anglePerSegment = 360f / targetCircle.SegmentCount;
                float worldSectorAngle = _activeArea.SectorIndex * anglePerSegment;
                
                float targetLocalAngle = Mathf.DeltaAngle(targetCircle.transform.eulerAngles.z, worldSectorAngle);
                segment.transform.localRotation = Quaternion.Euler(0, 0, targetLocalAngle);
                
                // Update our active segments tracking to match the new ownership
                _activeSegments[i] = segment;
            }
        }
    }
}