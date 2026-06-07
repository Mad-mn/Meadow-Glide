using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using Feature.CircleModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;
using Feature.TrackMoveModule.Scripts;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using AudioType = Feature.SoundModule.Scripts.AudioType;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideSegmentService : ISlideSegmentService, ITickable, IInitializable, IDisposable {
        private readonly IInputService _inputService;
        private readonly IInteractionStateService _interactionState;
        private readonly ICameraService _cameraService;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly GameCircleModel _circleModel;
        private readonly SlideAreaModel _slideAreaModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly LevelModel _levelModel;
        private readonly IAudioService _audioService;
        private readonly IVibrationService _vibrationService;
        private CircleParamsConfig _circleParamsConfig;
        private readonly List<CircleController> _circles = new List<CircleController>();

        private Camera _mainCamera;
        private SlideArea _activeArea;
        private float _startRadius;
        private Vector3 _slideDirection;
        private bool _isBlockedByStatus;
        private readonly List<CircleSegment> _activeSegments = new List<CircleSegment>();
        private readonly List<float> _baseIndices = new List<float>();
        private readonly List<CircleSegment> _ghosts = new List<CircleSegment>();
        private List<CircleController> _sortedCircles = new List<CircleController>();
        private bool _isSnapping;

        public bool IsSliding =>
            _activeArea != null;

        public SlideSegmentService(IInputService inputService, IInteractionStateService interactionState, ICameraService cameraService,
            UniTask<CircleParamsConfig> circleParamsConfigTask, GameCircleModel circleModel, SlideAreaModel slideAreaModel, MoveTrackModel moveTrackModel,
            LevelModel levelModel, IAudioService audioService, IVibrationService vibrationService) {
            _inputService = inputService;
            _interactionState = interactionState;
            _cameraService = cameraService;
            _circleParamsConfigTask = circleParamsConfigTask;
            _circleModel = circleModel;
            _slideAreaModel = slideAreaModel;
            _moveTrackModel = moveTrackModel;
            _levelModel = levelModel;
            _audioService = audioService;
            _vibrationService = vibrationService;
        }

        public async void Initialize() {
            _levelModel.OnLevelStart += OnLevelStart;
            _levelModel.OnLevelEnd += OnLevelEnd;
            _circleParamsConfig = await _circleParamsConfigTask;
        }

        private void OnLevelStart() {
            _inputService.PointerDown += OnPointerDown;
            _inputService.PointerUp += OnPointerUp;
        }

        public void Dispose() {
            _levelModel.OnLevelStart -= OnLevelStart;
            _levelModel.OnLevelEnd -= OnLevelEnd;
            OnLevelEnd();
        }

        private void OnLevelEnd() {
            _inputService.PointerDown -= OnPointerDown;
            _inputService.PointerUp -= OnPointerUp;
            ClearGhosts();
        }

        public void RegisterCircle(CircleController circle) {
            _circles.Add(circle);
            _sortedCircles = _circles.OrderBy(c => c.Radius)
                .ToList();
        }

        public void UpdateSegmentsInAreas() {
            List<CircleController> sortedCircles = _circleModel.Circles.OrderBy(c => c.Radius)
                .ToList();

            HashSet<CircleSegment> segmentsInAreas = new HashSet<CircleSegment>();
            IReadOnlyList<SlideArea> spawnedAreas = _slideAreaModel.SpawnedAreas;

            for (int circleIdx = 0; circleIdx < sortedCircles.Count; circleIdx++) {
                var circle = sortedCircles[circleIdx];
                foreach (var segment in circle.SpawnedSegments) {
                    float worldAngle = (circle.transform.eulerAngles.z + segment.transform.localEulerAngles.z) % 360;
                    worldAngle = (worldAngle + 360) % 360;

                    foreach (var area in spawnedAreas) {
                        if (circleIdx >= area.StartCircleIndex && circleIdx <= area.EndCircleIndex) {
                            float angleStep = 360f / area.TotalSegments;
                            float areaCenterAngle = area.SectorIndex * angleStep;

                            if (Mathf.Abs(Mathf.DeltaAngle(worldAngle, areaCenterAngle)) < 0.1f) {
                                segmentsInAreas.Add(segment);
                                break;
                            }
                        }
                    }
                }
            }

            _slideAreaModel.UpdateSegmentsInAreas(segmentsInAreas);
        }

        public void Clear() {
            _circles.Clear();
            _sortedCircles.Clear();
            ClearGhosts();
        }

        private void ClearGhosts() {
            foreach (var ghost in _ghosts) {
                if (ghost != null)
                    UnityEngine.Object.Destroy(ghost.gameObject);
            }

            _ghosts.Clear();
        }

        private void OnPointerDown() {
            if (_moveTrackModel.MovesLeft <= 0)
                return;

            if (_interactionState.IsRotationActive) {
                return;
            }
            
            if(_isSnapping)
                return;

            TrySlideSegments();
        }

        private void TrySlideSegments() {
            if (_mainCamera == null) {
                _mainCamera = _cameraService.CameraObject;
            }

            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            worldPos.z = 0;

            _activeArea = FindSlideArea(worldPos);

            if (_activeArea != null) {
                _interactionState.IsSlideActive = true;
                _slideDirection = worldPos.sqrMagnitude > 0.0001f
                    ? worldPos.normalized
                    : Vector3.up;

                _startRadius = Vector3.Dot(worldPos, _slideDirection);
                PrepareSegments(_activeArea);
                _slideAreaModel.ChangeSlideState(true);
                PlaySoundAndVibrationOnInteract(true);
            }
        }

        private SlideArea FindSlideArea(Vector3 worldPos) {
            foreach (var area in _slideAreaModel.SpawnedAreas) {
                var collider = area.GetComponent<PolygonCollider2D>();
                if (collider.OverlapPoint(worldPos)) {
                    return area;
                }
            }

            return null;
        }

        private void PrepareSegments(SlideArea area) {
            _activeSegments.Clear();
            _baseIndices.Clear();
            ClearGhosts();
            _isBlockedByStatus = false;

            int start = area.StartCircleIndex;
            int end = area.EndCircleIndex;

            bool isFilterColors = area.Status == SlideAreaStatus.FilterColors;
            var filterColors = area.FilterColors;

            for (int i = start; i <= end; i++) {
                if (i >= _sortedCircles.Count)
                    break;

                CircleController circle = _sortedCircles[i];
                float anglePerSegment = 360f / circle.SegmentCount;
                float worldSectorAngle = area.SectorIndex * anglePerSegment;

                var segment = circle.GetSegmentAtAngle(worldSectorAngle);
                if (segment != null) {
                    _activeSegments.Add(segment);
                    _baseIndices.Add(i);

                    if (segment.IsBlocked)
                        _isBlockedByStatus = true;

                    if (isFilterColors) {
                        if (filterColors == null || !filterColors.Contains(segment.ColorType)) {
                            _isBlockedByStatus = true;
                        }
                    }

                    var ghost = UnityEngine.Object.Instantiate(segment, segment.transform.parent);
                    ghost.gameObject.name = segment.gameObject.name + "_Ghost";

                    // CRITICAL: Clone the config so ghosts don't share state with originals
                    var configClone = segment.GetConfig()
                        .Clone();

                    ghost.SetConfig(configClone);

                    ghost.SetVisible(false);
                    ghost.SetSortingOrder(segment.GetSortingOrder() - 1);
                    ghost.HideStatusIcon();
                    _ghosts.Add(ghost);
                }
            }

            _slideAreaModel.SetupActiveSegments(_activeSegments);
        }

        public void Tick() {
            if (_activeArea == null)
                return;

            if (_mainCamera == null)
                _mainCamera = _cameraService.CameraObject;

            if (!_inputService.IsPointerPressed)
                return;

            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            worldPos.z = 0;

            float currentRadius = Vector3.Dot(worldPos, _slideDirection);

            if (_isBlockedByStatus) {
                // If the user tries to move significantly, trigger the shake
                if (Mathf.Abs(currentRadius - _startRadius) > 0.05f) {
                    foreach (var segment in _activeSegments) {
                        if (segment.IsBlocked) {
                            segment.TriggerBlockedAnimation();
                        }
                    }

                    if (_activeArea != null && _activeArea.Status == SlideAreaStatus.FilterColors) {
                        _activeArea.TriggerBlockedAnimation();
                    }
                }

                return;
            }

            UpdateSegmentsVisuals(currentRadius);
        }

        private void UpdateSegmentsVisuals(float currentRadius) {
            int count = _activeSegments.Count;
            if (count == 0 || _circleParamsConfig == null)
                return;

            int subsetCount = count;
            int startIdx = _activeArea.StartCircleIndex;
            int endIdx = startIdx + subsetCount - 1;

            float rStart = _circleParamsConfig.GetRadius(startIdx);
            float rEnd = _circleParamsConfig.GetRadius(endIdx);
            float rLimIn = (rStart + _circleParamsConfig.GetRadius(startIdx - 1)) / 2f;
            float rLimOut = (rEnd + _circleParamsConfig.GetRadius(endIdx + 1)) / 2f;

            float startVirtualIdx = _circleParamsConfig.GetVirtualIndex(_startRadius);
            float currentVirtualIdx = _circleParamsConfig.GetVirtualIndex(currentRadius);
            float deltaIndex = currentVirtualIdx - startVirtualIdx;

            for (int i = 0; i < count; i++) {
                // i is the index in the active subset (0 to subsetCount - 1)
                float virtualSubsetIdx = i + deltaIndex;
                float wrappedSubsetIdx = Mathf.Repeat(virtualSubsetIdx, subsetCount);

                // Main segment
                var segment = _activeSegments[i];
                float finalCircleIdx = startIdx + wrappedSubsetIdx;
                float r = Mathf.Max(0, _circleParamsConfig.GetRadius(finalCircleIdx));

                float baseWidth = _circleParamsConfig.GetWidth(finalCircleIdx);
                float fade = GetGeometricFade(r, rStart, rEnd, rLimIn, rLimOut);

                segment.SetWidth(baseWidth * fade, true);
                segment.SetRadius(r);
                segment.SetVisible(fade > 0.01f);

                // Ghost segment
                float mid = subsetCount / 2f;
                float ghostSubsetIdx = wrappedSubsetIdx > mid
                    ? wrappedSubsetIdx - subsetCount
                    : wrappedSubsetIdx + subsetCount;

                float finalGhostCircleIdx = startIdx + ghostSubsetIdx;
                float gr = Mathf.Max(0, _circleParamsConfig.GetRadius(finalGhostCircleIdx));

                float ghostFade = GetGeometricFade(gr, rStart, rEnd, rLimIn, rLimOut);

                var ghost = _ghosts[i];
                ghost.SetWidth(_circleParamsConfig.GetWidth(finalGhostCircleIdx) * ghostFade, true);
                ghost.SetRadius(gr);
                ghost.SetVisible(ghostFade > 0.01f);
                ghost.HideStatusIcon();
                ghost.transform.localRotation = segment.transform.localRotation;
            }
        }

        private float GetGeometricFade(float radius, float rStart, float rEnd, float rLimIn, float rLimOut) {
            if (radius < rStart)
                return Mathf.InverseLerp(rLimIn, rStart, radius);

            if (radius > rEnd)
                return Mathf.InverseLerp(rLimOut, rEnd, radius);

            return 1.0f;
        }

        private void OnPointerUp() {
            SlideArea areaToSnap = _activeArea; // Cache it because SnapSegments uses it and OnPointerUp clears it
            if (_activeArea != null) {
                SnapSegments(areaToSnap)
                    .Forget();

                PlaySoundAndVibrationOnInteract(false);
                _activeArea = null;
            }

            _interactionState.IsSlideActive = false;
        }

        private async UniTaskVoid SnapSegments(SlideArea area) {
            if(_isSnapping)
                return;
            _isSnapping = true;
            int count = _activeSegments.Count;
            if (count == 0 || _circleParamsConfig == null || _isBlockedByStatus) {
                _slideAreaModel.ChangeSlideState(false);
                ClearGhosts();
                _activeSegments.Clear();
                _baseIndices.Clear();
                return;
            }

            int startIdx = area.StartCircleIndex;
            int subsetCount = count;

            float firstSegCurrentR = _activeSegments[0].Radius;
            float currentVirtualIdx = _circleParamsConfig.GetVirtualIndex(firstSegCurrentR);
            float rawDeltaIndex = currentVirtualIdx - _baseIndices[0];

            float normalizedDeltaIndex = ((rawDeltaIndex % subsetCount) + subsetCount) % subsetCount;
            if (normalizedDeltaIndex > subsetCount / 2f)
                normalizedDeltaIndex -= subsetCount;

            int shift = Mathf.RoundToInt(normalizedDeltaIndex);

            ApplyShift(area, shift);

            float duration = 0.2f;
            float elapsed = 0;

            float[] currentVirtualIndices = new float[count];
            float[] targetVirtualIndices = new float[count];

            DisableGhosts(count, startIdx, subsetCount, currentVirtualIndices, targetVirtualIndices);

            int endIdx = startIdx + subsetCount - 1;
            float rStart = _circleParamsConfig.GetRadius(startIdx);
            float rEnd = _circleParamsConfig.GetRadius(endIdx);
            float rLimIn = (rStart + _circleParamsConfig.GetRadius(startIdx - 1)) / 2f;
            float rLimOut = (rEnd + _circleParamsConfig.GetRadius(endIdx + 1)) / 2f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3);

                for (int i = 0; i < count; i++) {
                    SnapSegment(i, currentVirtualIndices, targetVirtualIndices, t, rStart, rEnd, rLimIn, rLimOut);
                }

                await UniTask.Yield();
            }

            for (int i = 0; i < count; i++) {
                SegmentSetupAfterSnap(i, startIdx);
            }

            _circleModel.SegmentsChanged();
            ClearGhosts();
            _activeSegments.Clear();
            _baseIndices.Clear();
            _slideAreaModel.ChangeSlideState(false);
            _isSnapping = false;
        }
        
        private void PlaySoundAndVibrationOnInteract(bool isSliding) {
            _audioService.PlaySound(isSliding ? AudioType.CircleStartInteraction : AudioType.CircleStopInteraction);
            _vibrationService.PlayVibration(VibrationType.Low);
        }

        private void SegmentSetupAfterSnap(int i, int startIdx) {
            if (_activeSegments[i] != null) {
                float finalIdx = startIdx + i;
                _activeSegments[i]
                    .SetWidth(_circleParamsConfig.GetWidth(finalIdx));

                _activeSegments[i]
                    .SetRadius(_circleParamsConfig.GetRadius(finalIdx));

                _activeSegments[i]
                    .SetVisible(true);
            }
        }

        private void SnapSegment(int i, float[] currentVirtualIndices, float[] targetVirtualIndices, float t, float rStart, float rEnd, float rLimIn, float rLimOut) {
            var seg = _activeSegments[i];
            if (seg == null)
                return;

            float idx = Mathf.Lerp(currentVirtualIndices[i], targetVirtualIndices[i], t);
            float r = Mathf.Max(0, _circleParamsConfig.GetRadius(idx));

            float fade = GetGeometricFade(r, rStart, rEnd, rLimIn, rLimOut);
            float clampedR = Mathf.Clamp(r, rLimIn, rLimOut);

            seg.SetWidth(_circleParamsConfig.GetWidth(idx) * fade);
            seg.SetRadius(clampedR);
            seg.SetVisible(fade > 0.01f);
        }

        private void DisableGhosts(int count, int startIdx, int subsetCount, float[] currentVirtualIndices, float[] targetVirtualIndices) {
            for (int i = 0; i < count; i++) {
                float currentIdx = _circleParamsConfig.GetVirtualIndex(_activeSegments[i].Radius);
                float targetIdx = startIdx + i;

                // Wrap startIdx to be closest to targetIdx for smooth lerp within subset wrapping
                if (currentIdx - targetIdx > subsetCount / 2f)
                    currentIdx -= subsetCount;
                else if (targetIdx - currentIdx > subsetCount / 2f)
                    currentIdx += subsetCount;

                currentVirtualIndices[i] = currentIdx;
                targetVirtualIndices[i] = targetIdx;
                _ghosts[i]
                    .SetVisible(false);
            }
        }

        private void ApplyShift(SlideArea area, int shift) {
            if (shift == 0)
                return;

            int count = _activeSegments.Count;
            int startIdx = area.StartCircleIndex;
            CircleSegment[] shiftedSegments = new CircleSegment[count];
            for (int i = 0; i < count; i++) {
                int newIndex = ((i + shift) % count + count) % count;
                shiftedSegments[newIndex] = _activeSegments[i];
            }

            for (int i = 0; i < count; i++) {
                var targetCircle = _sortedCircles[startIdx + i];
                var segment = shiftedSegments[i];

                if (segment.transform.parent != null) {
                    var oldCircle = segment.transform.parent.GetComponent<CircleController>();
                    if (oldCircle != null)
                        oldCircle.RemoveSegment(segment);
                }

                targetCircle.AddSegment(segment);

                float anglePerSegment = 360f / targetCircle.SegmentCount;
                float worldSectorAngle = area.SectorIndex * anglePerSegment;

                float targetLocalAngle = Mathf.DeltaAngle(targetCircle.transform.eulerAngles.z, worldSectorAngle);
                segment.transform.localRotation = Quaternion.Euler(0, 0, targetLocalAngle);

                _activeSegments[i] = segment;
            }
        }
    }
}