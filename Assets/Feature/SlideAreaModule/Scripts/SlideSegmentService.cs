using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using UnityEngine;
using Zenject;
using AudioType = Feature.SoundModule.Scripts.AudioType;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideSegmentService : ISlideSegmentService, ITickable, IInitializable, IDisposable {
        private readonly IInputService _inputService;
        private readonly IInteractionStateService _interactionState;
        private readonly ICameraService _cameraService;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private readonly StripModel _stripModel;
        private readonly SlideAreaModel _slideAreaModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly LevelModel _levelModel;
        private readonly IAudioService _audioService;
        private readonly IVibrationService _vibrationService;
        private CircleParamsConfig _circleParamsConfig;
        private readonly List<StripController> _strips = new List<StripController>();

        private Camera _mainCamera;
        private SlideArea _activeArea;
        private float _startY;
        private bool _isBlockedByStatus;
        private readonly List<StripSegment> _activeSegments = new List<StripSegment>();
        private readonly List<float> _baseIndices = new List<float>();
        private readonly List<StripSegment> _ghosts = new List<StripSegment>();
        private List<StripController> _sortedStrips = new List<StripController>();
        private bool _isSnapping;

        public bool IsSliding => _activeArea != null;

        public SlideSegmentService(IInputService inputService, IInteractionStateService interactionState, ICameraService cameraService,
            UniTask<CircleParamsConfig> circleParamsConfigTask, StripModel stripModel, SlideAreaModel slideAreaModel, MoveTrackModel moveTrackModel,
            LevelModel levelModel, IAudioService audioService, IVibrationService vibrationService) {
            _inputService = inputService;
            _interactionState = interactionState;
            _cameraService = cameraService;
            _circleParamsConfigTask = circleParamsConfigTask;
            _stripModel = stripModel;
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
        }

        public void RegisterStrip(StripController strip) {
            _strips.Add(strip);
            _sortedStrips = _strips.OrderBy(strip => strip.PositionIndex).ToList();
        }

        public void UpdateSegmentsInAreas() {
            HashSet<IGameSegment> segmentsInAreas = new HashSet<IGameSegment>();

            foreach (SlideArea area in _slideAreaModel.SpawnedAreas) {
                for (int stripIdx = area.StartCircleIndex; stripIdx <= area.EndCircleIndex; stripIdx++) {
                    StripController strip = _sortedStrips.FirstOrDefault(s => s.PositionIndex == stripIdx);
                    if (strip == null)
                        continue;

                    StripSegment segment = strip.GetSegmentAtColumn(area.SectorIndex);
                    if (segment != null)
                        segmentsInAreas.Add(segment);
                }
            }

            _slideAreaModel.UpdateSegmentsInAreas(segmentsInAreas);
        }

        public void Clear() {
            _strips.Clear();
            _sortedStrips.Clear();
            ClearGhosts();
        }

        private void ClearGhosts() {
            foreach (StripSegment ghost in _ghosts) {
                if (ghost != null)
                    UnityEngine.Object.Destroy(ghost.gameObject);
            }

            _ghosts.Clear();
        }

        private void OnPointerDown() {
            if (_moveTrackModel.MovesLeft <= 0)
                return;

            if (_interactionState.IsRotationActive)
                return;

            if (_isSnapping)
                return;

            TrySlideSegments();
        }

        private void TrySlideSegments() {
            if (_mainCamera == null)
                _mainCamera = _cameraService.CameraObject;

            Vector2 screenPos = _inputService.PointerPosition;
            Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_mainCamera.transform.position.z));
            worldPos.z = 0;

            _activeArea = FindSlideArea(worldPos);

            if (_activeArea != null) {
                _interactionState.IsSlideActive = true;
                _startY = worldPos.y;
                PrepareSegments(_activeArea);
                _slideAreaModel.ChangeSlideState(true);
                PlaySoundAndVibrationOnInteract(true);
            }
        }

        private SlideArea FindSlideArea(Vector3 worldPos) {
            foreach (SlideArea area in _slideAreaModel.SpawnedAreas) {
                PolygonCollider2D collider = area.GetComponent<PolygonCollider2D>();
                if (collider.OverlapPoint(worldPos))
                    return area;
            }

            return null;
        }

        private StripController GetStripByIndex(int stripIndex) {
            return _sortedStrips.FirstOrDefault(strip => strip.PositionIndex == stripIndex);
        }

        private void PrepareSegments(SlideArea area) {
            _activeSegments.Clear();
            _baseIndices.Clear();
            ClearGhosts();
            _isBlockedByStatus = false;

            int start = area.StartCircleIndex;
            int end = area.EndCircleIndex;
            bool isFilterColors = area.Status == SlideAreaStatus.FilterColors;
            List<CircleColorType> filterColors = area.FilterColors;

            for (int i = start; i <= end; i++) {
                StripController strip = GetStripByIndex(i);
                if (strip == null)
                    break;
                StripSegment segment = strip.GetSegmentAtColumn(area.SectorIndex);
                if (segment == null)
                    continue;

                _activeSegments.Add(segment);
                _baseIndices.Add(i);

                if (segment.IsBlocked)
                    _isBlockedByStatus = true;

                if (isFilterColors && (filterColors == null || !filterColors.Contains(segment.ColorType)))
                    _isBlockedByStatus = true;

                StripSegment ghost = UnityEngine.Object.Instantiate(segment, segment.transform.parent);
                ghost.gameObject.name = segment.gameObject.name + "_Ghost";
                ghost.SetConfig(segment.GetConfig().Clone());
                ghost.SetVisible(false);
                ghost.SetSortingOrder(segment.GetSortingOrder() - 1);
                ghost.HideStatusIcon();
                _ghosts.Add(ghost);
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

            float currentY = worldPos.y;

            if (_isBlockedByStatus) {
                if (Mathf.Abs(currentY - _startY) > 0.05f) {
                    foreach (StripSegment segment in _activeSegments) {
                        if (segment.IsBlocked)
                            segment.TriggerBlockedAnimation();
                    }

                    if (_activeArea.Status == SlideAreaStatus.FilterColors)
                        _activeArea.TriggerBlockedAnimation();
                }

                return;
            }

            UpdateSegmentsVisuals(currentY);
        }

        private void UpdateSegmentsVisuals(float currentY) {
            int count = _activeSegments.Count;
            if (count == 0 || _circleParamsConfig == null)
                return;

            int subsetCount = count;
            int startIdx = _activeArea.StartCircleIndex;
            int endIdx = startIdx + subsetCount - 1;

            float yStart = _circleParamsConfig.GetStripCenterY(startIdx);
            float yEnd = _circleParamsConfig.GetStripCenterY(endIdx);
            float yLimIn = (yStart + _circleParamsConfig.GetStripCenterY(startIdx - 1)) * 0.5f;
            float yLimOut = (yEnd + _circleParamsConfig.GetStripCenterY(endIdx + 1)) * 0.5f;

            float startVirtualIdx = _circleParamsConfig.GetStripVirtualIndex(_startY);
            float currentVirtualIdx = _circleParamsConfig.GetStripVirtualIndex(currentY);
            float deltaIndex = currentVirtualIdx - startVirtualIdx;
            float uniformHeight = _circleParamsConfig.GetUniformSegmentThickness();

            for (int i = 0; i < count; i++) {
                StripController homeStrip = GetStripByIndex(startIdx + i);
                if (homeStrip == null)
                    continue;

                float virtualSubsetIdx = i + deltaIndex;
                float wrappedSubsetIdx = Mathf.Repeat(virtualSubsetIdx, subsetCount);

                StripSegment segment = _activeSegments[i];
                float finalStripIdx = startIdx + wrappedSubsetIdx;
                float y = _circleParamsConfig.GetStripYFromVirtualIndex(finalStripIdx);
                float fade = GetGeometricFade(y, yStart, yEnd, yLimIn, yLimOut);

                segment.SetWidth(uniformHeight * fade, true);
                segment.SetRadius(y - homeStrip.CenterY);
                segment.SetVisible(fade > 0.01f);

                float mid = subsetCount / 2f;
                float ghostSubsetIdx = wrappedSubsetIdx > mid
                    ? wrappedSubsetIdx - subsetCount
                    : wrappedSubsetIdx + subsetCount;

                float finalGhostStripIdx = startIdx + ghostSubsetIdx;
                float ghostY = _circleParamsConfig.GetStripYFromVirtualIndex(finalGhostStripIdx);
                float ghostFade = GetGeometricFade(ghostY, yStart, yEnd, yLimIn, yLimOut);

                StripSegment ghost = _ghosts[i];
                ghost.SetWidth(uniformHeight * ghostFade, true);
                ghost.SetRadius(ghostY - homeStrip.CenterY);
                ghost.SetVisible(ghostFade > 0.01f);
                ghost.HideStatusIcon();
            }
        }

        private static float GetGeometricFade(float y, float yStart, float yEnd, float yLimIn, float yLimOut) {
            if (y > yStart)
                return Mathf.InverseLerp(yLimIn, yStart, y);

            if (y < yEnd)
                return Mathf.InverseLerp(yLimOut, yEnd, y);

            return 1.0f;
        }

        private void OnPointerUp() {
            SlideArea areaToSnap = _activeArea;
            if (_activeArea != null) {
                SnapSegments(areaToSnap).Forget();
                PlaySoundAndVibrationOnInteract(false);
                _activeArea = null;
            }
            else {
                _interactionState.IsSlideActive = false;
            }
        }

        private async UniTaskVoid SnapSegments(SlideArea area) {
            if (_isSnapping)
                return;

            _isSnapping = true;
            int count = _activeSegments.Count;
            if (count == 0 || _circleParamsConfig == null || _isBlockedByStatus) {
                _slideAreaModel.ChangeSlideState(false);
                ClearGhosts();
                _activeSegments.Clear();
                _baseIndices.Clear();
                _isSnapping = false;
                _interactionState.IsSlideActive = false;
                return;
            }

            int startIdx = area.StartCircleIndex;
            int subsetCount = count;
            float uniformHeight = _circleParamsConfig.GetUniformSegmentThickness();

            float firstSegCurrentY = GetStripByIndex(startIdx).CenterY + _activeSegments[0].Radius;
            float currentVirtualIdx = _circleParamsConfig.GetStripVirtualIndex(firstSegCurrentY);
            float rawDeltaIndex = currentVirtualIdx - _baseIndices[0];

            float normalizedDeltaIndex = ((rawDeltaIndex % subsetCount) + subsetCount) % subsetCount;
            if (normalizedDeltaIndex > subsetCount / 2f)
                normalizedDeltaIndex -= subsetCount;

            int shift = Mathf.RoundToInt(normalizedDeltaIndex);
            ApplyShift(area, shift);

            const float duration = 0.2f;
            float elapsed = 0f;
            float[] currentVirtualIndices = new float[count];
            float[] targetVirtualIndices = new float[count];

            DisableGhosts(count, startIdx, subsetCount, currentVirtualIndices, targetVirtualIndices);

            int endIdx = startIdx + subsetCount - 1;
            float yStart = _circleParamsConfig.GetStripCenterY(startIdx);
            float yEnd = _circleParamsConfig.GetStripCenterY(endIdx);
            float yLimIn = (yStart + _circleParamsConfig.GetStripCenterY(startIdx - 1)) * 0.5f;
            float yLimOut = (yEnd + _circleParamsConfig.GetStripCenterY(endIdx + 1)) * 0.5f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3f);

                for (int i = 0; i < count; i++)
                    SnapSegment(i, currentVirtualIndices, targetVirtualIndices, t, startIdx, yStart, yEnd, yLimIn, yLimOut, uniformHeight);

                await UniTask.Yield();
            }

            for (int i = 0; i < count; i++)
                SegmentSetupAfterSnap(i, startIdx, uniformHeight);

            UpdateSegmentsInAreas();
            _stripModel.SegmentsChanged();
            ClearGhosts();
            _activeSegments.Clear();
            _baseIndices.Clear();
            _slideAreaModel.ChangeSlideState(false);
            _isSnapping = false;
            _interactionState.IsSlideActive = false;
        }

        private void PlaySoundAndVibrationOnInteract(bool isSliding) {
            _audioService.PlaySound(isSliding ? AudioType.CircleStartInteraction : AudioType.CircleStopInteraction);
            _vibrationService.PlayVibration(VibrationType.Low);
        }

        private void SegmentSetupAfterSnap(int i, int startIdx, float uniformHeight) {
            if (_activeSegments[i] == null)
                return;

            StripController strip = GetStripByIndex(startIdx + i);
            _activeSegments[i].SetWidth(uniformHeight);
            _activeSegments[i].SetRadius(0f);
            _activeSegments[i].SetVisible(true);
        }

        private void SnapSegment(int i, float[] currentVirtualIndices, float[] targetVirtualIndices, float t, int startIdx,
            float yStart, float yEnd, float yLimIn, float yLimOut, float uniformHeight) {
            StripSegment seg = _activeSegments[i];
            if (seg == null)
                return;

            float idx = Mathf.Lerp(currentVirtualIndices[i], targetVirtualIndices[i], t);
            float y = _circleParamsConfig.GetStripYFromVirtualIndex(idx);
            float fade = GetGeometricFade(y, yStart, yEnd, yLimIn, yLimOut);
            float clampedY = Mathf.Clamp(y, yLimOut, yLimIn);

            seg.SetWidth(uniformHeight * fade);
            seg.SetRadius(clampedY - GetStripByIndex(startIdx + i).CenterY);
            seg.SetVisible(fade > 0.01f);
        }

        private void DisableGhosts(int count, int startIdx, int subsetCount, float[] currentVirtualIndices, float[] targetVirtualIndices) {
            for (int i = 0; i < count; i++) {
                float stripCenter = GetStripByIndex(startIdx + i).CenterY;
                float currentIdx = _circleParamsConfig.GetStripVirtualIndex(stripCenter + _activeSegments[i].Radius);
                float targetIdx = startIdx + i;

                if (currentIdx - targetIdx > subsetCount / 2f)
                    currentIdx -= subsetCount;
                else if (targetIdx - currentIdx > subsetCount / 2f)
                    currentIdx += subsetCount;

                currentVirtualIndices[i] = currentIdx;
                targetVirtualIndices[i] = targetIdx;
                _ghosts[i].SetVisible(false);
            }
        }

        private void ApplyShift(SlideArea area, int shift) {
            if (shift == 0)
                return;

            int count = _activeSegments.Count;
            int startIdx = area.StartCircleIndex;
            int sectorIndex = area.SectorIndex;

            StripSegment[] shiftedSegments = new StripSegment[count];
            StripController[] sourceStrips = new StripController[count];
            int[] targetSlotIndices = new int[count];

            for (int i = 0; i < count; i++) {
                int newIndex = ((i + shift) % count + count) % count;
                shiftedSegments[newIndex] = _activeSegments[i];
            }

            for (int i = 0; i < count; i++) {
                StripSegment segment = shiftedSegments[i];
                if (segment.transform.parent != null)
                    sourceStrips[i] = segment.transform.parent.GetComponent<StripController>();

                StripController targetStrip = GetStripByIndex(startIdx + i);
                float segmentSpan = targetStrip.GetSegmentSpan();
                targetSlotIndices[i] = Mod(
                    Mathf.FloorToInt((sectorIndex + targetStrip.ScrollOffset / segmentSpan)),
                    targetStrip.SegmentCount);
            }

            for (int i = 0; i < count; i++) {
                if (sourceStrips[i] != null)
                    sourceStrips[i].RemoveSegment(shiftedSegments[i]);
            }

            for (int i = 0; i < count; i++) {
                StripController targetStrip = GetStripByIndex(startIdx + i);
                targetStrip.AddSegment(shiftedSegments[i], targetSlotIndices[i]);
                _activeSegments[i] = shiftedSegments[i];
            }
        }

        private static int Mod(int value, int count) {
            if (count <= 0) return 0;
            int result = value % count;
            return result < 0 ? result + count : result;
        }
    }
}
