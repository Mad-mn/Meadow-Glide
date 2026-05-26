using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.InputModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideSegmentService : ISlideSegmentService, ITickable, IInitializable, IDisposable {
        private readonly IInputService _inputService;
        private readonly ISlideAreaService _slideAreaService;
        private readonly IInteractionStateService _interactionState;
        private readonly List<CircleController> _circles = new List<CircleController>();
        
        private SlideArea _activeArea;
        private float _startRadius;
        private List<CircleSegment> _activeSegments = new List<CircleSegment>();
        private List<float> _baseRadii = new List<float>();
        private List<CircleSegment> _ghosts = new List<CircleSegment>();

        public bool IsSliding => _activeArea != null;

        public SlideSegmentService(
            IInputService inputService, 
            ISlideAreaService slideAreaService,
            IInteractionStateService interactionState) {
            _inputService = inputService;
            _slideAreaService = slideAreaService;
            _interactionState = interactionState;
        }

        public void Initialize() {
            _inputService.PointerDown += OnPointerDown;
            _inputService.PointerUp += OnPointerUp;
        }

        public void Dispose() {
            _inputService.PointerDown -= OnPointerDown;
            _inputService.PointerUp -= OnPointerUp;
            ClearGhosts();
        }

        public void RegisterCircle(CircleController circle) {
            _circles.Add(circle);
        }

        public void Clear() {
            _circles.Clear();
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

            Vector2 screenPos = _inputService.PointerPosition;
            var camera = Camera.main ?? GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
            if (camera == null) {
                return;
            }

            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0;

            _activeArea = FindSlideArea(worldPos);

            if (_activeArea != null) {
                _interactionState.IsSlideActive = true;
                _startRadius = worldPos.magnitude;
                PrepareSegments(_activeArea.SectorIndex);
                ((SlideAreaService)_slideAreaService).IsSliding = true;
            }
        }

        private SlideArea FindSlideArea(Vector3 worldPos) {
            var slideAreaService = (SlideAreaService)_slideAreaService;
            foreach (var area in slideAreaService.SpawnedAreas) {
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
            
            var sortedCircles = _circles.OrderBy(c => c.Radius).ToList();
            
            foreach (var circle in sortedCircles) {
                float anglePerSegment = 360f / circle.SegmentCount;
                float worldSectorAngle = sectorIndex * anglePerSegment;
                
                var segment = circle.GetSegmentAtAngle(worldSectorAngle);
                if (segment != null) {
                    _activeSegments.Add(segment);
                    _baseRadii.Add(circle.Radius);
                    
                    var ghost = UnityEngine.Object.Instantiate(segment, segment.transform.parent);
                    ghost.gameObject.name = segment.gameObject.name + "_Ghost";
                    ghost.SetRadius(-100); 
                    _ghosts.Add(ghost);
                }
            }
        }

        public void Tick() {
            if (_activeArea == null) return;

            Vector2 screenPos = _inputService.PointerPosition;
            var camera = Camera.main ?? GameObject.FindObjectsByType<Camera>(FindObjectsSortMode.None).FirstOrDefault();
            if (camera == null) return;

            Vector3 worldPos = camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -camera.transform.position.z));
            worldPos.z = 0;

            float currentRadius = worldPos.magnitude;
            float delta = currentRadius - _startRadius;

            UpdateSegmentsVisuals(delta);
        }

        private void UpdateSegmentsVisuals(float delta) {
            if (_activeSegments.Count == 0) return;

            float minR = _baseRadii.Min();
            float maxR = _baseRadii.Max();
            float step = _baseRadii.Count > 1 ? (_baseRadii[1] - _baseRadii[0]) : 1.0f; 
            float totalSpan = maxR - minR + step;

            for (int i = 0; i < _activeSegments.Count; i++) {
                float virtualR = _baseRadii[i] + delta;
                
                float wrappedR = minR + Mathf.Repeat(virtualR - minR, totalSpan);
                _activeSegments[i].SetRadius(wrappedR);
                
                float ghostR;
                if (wrappedR > (minR + maxR) / 2f) {
                    ghostR = wrappedR - totalSpan;
                } else {
                    ghostR = wrappedR + totalSpan;
                }
                _ghosts[i].SetRadius(ghostR);
                _ghosts[i].transform.localRotation = _activeSegments[i].transform.localRotation;
            }
        }

        private void OnPointerUp() {
            if (_activeArea != null) {
                SnapSegments().Forget();
                _activeArea = null;
                ((SlideAreaService)_slideAreaService).IsSliding = false;
            }
            _interactionState.IsSlideActive = false;
        }

        private async UniTaskVoid SnapSegments() {
            if (_activeSegments.Count == 0) return;

            float minR = _baseRadii.Min();
            float maxR = _baseRadii.Max();
            float step = _baseRadii.Count > 1 ? (_baseRadii[1] - _baseRadii[0]) : 1.0f;
            float totalSpan = maxR - minR + step;

            float firstSegCurrentR = _activeSegments[0].Radius;
            float rawDelta = firstSegCurrentR - _baseRadii[0];
            
            float normalizedDelta = ((rawDelta % totalSpan) + totalSpan) % totalSpan;
            if (normalizedDelta > totalSpan / 2f) normalizedDelta -= totalSpan;

            int shift = Mathf.RoundToInt(normalizedDelta / step);
            
            ApplyShift(shift);

            float duration = 0.2f;
            float elapsed = 0;
            
            int count = _activeSegments.Count;
            List<float> startRadii = _activeSegments.Select(s => s.Radius).ToList();
            var sortedCircles = _circles.OrderBy(c => c.Radius).ToList();
            
            List<float> targetRadii = new List<float>(count);
            for (int i = 0; i < count; i++) {
                int targetCircleIndex = ((i + shift) % count + count) % count;
                targetRadii.Add(sortedCircles[targetCircleIndex].Radius);
            }

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3);

                for (int i = 0; i < count; i++) {
                    if (_activeSegments[i] == null) continue;
                    float r = Mathf.Lerp(startRadii[i], targetRadii[i], t);
                    _activeSegments[i].SetRadius(r);
                    _ghosts[i].SetRadius(-100);
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
            if (shift == 0) {
                return;
            }
            
            var sortedCircles = _circles.OrderBy(c => c.Radius).ToList();
            int count = _activeSegments.Count;
            
            var shiftedSegments = new CircleSegment[count];
            for (int i = 0; i < count; i++) {
                int newIndex = ((i + shift) % count + count) % count;
                shiftedSegments[newIndex] = _activeSegments[i];
            }

            for (int i = 0; i < count; i++) {
                var targetCircle = sortedCircles[i];
                var segment = shiftedSegments[i];
                
                foreach (var circle in _circles) {
                    circle.RemoveSegment(segment);
                }
                
                targetCircle.AddSegment(segment);
                
                float anglePerSegment = 360f / targetCircle.SegmentCount;
                float worldSectorAngle = _activeArea.SectorIndex * anglePerSegment;
                
                float targetLocalAngle = Mathf.DeltaAngle(targetCircle.transform.eulerAngles.z, worldSectorAngle);
                segment.transform.localRotation = Quaternion.Euler(0, 0, targetLocalAngle);
            }
        }
    }
}