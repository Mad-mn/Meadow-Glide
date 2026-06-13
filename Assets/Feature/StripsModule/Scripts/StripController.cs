using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using UnityEngine;
using Zenject;

namespace Feature.StripsModule.Scripts {
    public class StripController : MonoBehaviour {
        private const int MaxWrapGhosts = 2;

        [SerializeField] private StripSegment _segmentPrefab;
        [SerializeField] private StripAnimationController _stripAnimationController;

        private readonly List<StripSegment> _spawnedSegments = new List<StripSegment>();
        private readonly List<StripSegment> _wrapGhosts = new List<StripSegment>();

        private CircleConfig _currentConfig;
        private ICircleColorService _circleColorService;
        private ISegmentStatusVisualDataProvider _statusVisualDataProvider;
        private float _segmentHeight;
        private float _stripLoopLength;
        private float _centerY;
        private int _positionIndex;
        private float _scrollOffset;

        public float CenterY => _centerY;
        public float ScrollOffset => _scrollOffset;
        public float StripLoopLength => _stripLoopLength;
        public int PositionIndex => _positionIndex;

        public bool IsCompleted {
            get {
                CircleColorType stripColorType = CircleColorType.None;
                bool completed = true;
                foreach (StripSegment segment in _spawnedSegments) {
                    if (stripColorType is CircleColorType.None) {
                        stripColorType = segment.ColorType;
                        continue;
                    }

                    if (segment.ColorType != stripColorType) {
                        completed = false;
                        break;
                    }
                }

                return completed;
            }
        }

        public int SegmentCount =>
            _currentConfig != null
                ? _currentConfig.SegmentCount
                : 0;

        public IReadOnlyList<StripSegment> SpawnedSegments => _spawnedSegments;

        [Inject]
        public void InjectDependencies(ICircleColorService colorService, ISegmentStatusVisualDataProvider statusVisualDataProvider) {
            _circleColorService = colorService;
            _statusVisualDataProvider = statusVisualDataProvider;
        }

        public void Setup(CircleConfig config, float segmentHeight, float stripLoopLength, float centerY, int positionIndex) {
            _currentConfig = config;
            _segmentHeight = segmentHeight;
            _stripLoopLength = stripLoopLength;
            _centerY = centerY;
            _positionIndex = positionIndex;
            _scrollOffset = 0f;
            transform.localPosition = new Vector3(0f, centerY, 0f);
            BuildStrip();
        }

        public float GetSegmentSpan() {
            return SegmentCount > 0 ? _stripLoopLength / SegmentCount : 0f;
        }

        public StripSegment GetSegmentAtColumn(int columnIndex) {
            if (_spawnedSegments.Count == 0)
                return null;

            float segmentSpan = GetSegmentSpan();
            var b = _scrollOffset / segmentSpan;
            var a = Mathf.FloorToInt((columnIndex + b));
           
            int slotIndex = Mod(a, SegmentCount);
            return _spawnedSegments[slotIndex];
        }

        public void SetScrollOffset(float offset, bool showWrapGhosts = false) {
            _scrollOffset = offset;
            ApplySegmentLayout(showWrapGhosts);
        }

        public void RemoveSegment(StripSegment segment) {
            _spawnedSegments.Remove(segment);
        }

        public void AddSegment(StripSegment segment) {
            segment.transform.SetParent(transform);
            _spawnedSegments.Add(segment);
            segment.SetWidth(_segmentHeight);
            segment.SetSpan(GetSegmentSpan() * 0.5f);
            segment.SetRadius(0f);
            ApplySegmentLayout(false);
        }

        public void AddSegment(StripSegment segment, int index) {
            segment.transform.SetParent(transform);

            int clampedIndex = Mathf.Clamp(index, 0, _spawnedSegments.Count);
            _spawnedSegments.Insert(clampedIndex, segment);

            segment.SetWidth(_segmentHeight);
            segment.SetSpan(GetSegmentSpan() * 0.5f);
            segment.SetRadius(0f);
            ApplySegmentLayout(false);
        }

        public void PlayCompletedAnimation(Action callback) {
            _stripAnimationController.PlayCompletedAnimation(callback);
        }

        public void ClearWrapGhosts() {
            foreach (StripSegment ghost in _wrapGhosts) {
                if (ghost != null)
                    Destroy(ghost.gameObject);
            }

            _wrapGhosts.Clear();
        }

        private void BuildStrip() {
            ClearStrip();

            if (_currentConfig == null || _segmentPrefab == null || _circleColorService == null) {
                Debug.LogError("StripController: Missing references or config!");
                return;
            }

            float halfSpan = GetSegmentSpan() * 0.5f;

            for (int i = 0; i < _currentConfig.SegmentCount; i++) {
                SegmentConfig segData;
                if (_currentConfig.Segments != null && i < _currentConfig.Segments.Count) {
                    var originalSeg = _currentConfig.Segments[i];
                    segData = new SegmentConfig {
                        ColorType = originalSeg.ColorType,
                        SegmentStatus = originalSeg.SegmentStatus,
                        Radius = 0f,
                        Angle = 0f
                    };
                }
                else {
                    segData = new SegmentConfig {
                        ColorType = CircleColorType.None,
                        SegmentStatus = SegmentStatus.Default,
                        Radius = 0f,
                        Angle = 0f
                    };
                }

                StripSegment segment = Instantiate(_segmentPrefab, transform);
                Color color = _circleColorService.GetColor(segData.ColorType);
                segment.Initialize(segData, color, _segmentHeight, halfSpan, _statusVisualDataProvider);
                _spawnedSegments.Add(segment);
            }

            ApplySegmentLayout(false);
        }

        private void ApplySegmentLayout(bool showWrapGhosts) {
            if (_spawnedSegments.Count == 0)
                return;

            float segmentSpan = GetSegmentSpan();
            float halfSpan = segmentSpan * 0.5f;
            float halfLoop = _stripLoopLength * 0.5f;

            ClearWrapGhosts();

            for (int i = 0; i < _spawnedSegments.Count; i++) {
                StripSegment segment = _spawnedSegments[i];
                float rawX = (i + 0.5f) * segmentSpan - _scrollOffset;
                float wrappedX = WrapHorizontal(rawX, _stripLoopLength) - halfLoop;
                segment.SetCenterX(wrappedX);
                segment.SetRadius(0f);

                if (showWrapGhosts) {
                    float segLeft = wrappedX - halfSpan;
                    float segRight = wrappedX + halfSpan;
                    float visLeft = Mathf.Max(segLeft, -halfLoop);
                    float visRight = Mathf.Min(segRight, halfLoop);

                    if (visLeft >= visRight) {
                        segment.SetVisible(false);
                    }
                    else {
                        segment.SetWidth(_segmentHeight, true);
                        segment.SetVisibleSpan(visLeft - wrappedX, visRight - wrappedX);
                        segment.SetVisible(true);

                        bool isClipped = segLeft < -halfLoop || segRight > halfLoop;
                        if (isClipped)
                            segment.HideStatusIcon();
                    }

                    if (segLeft < -halfLoop) {
                        float overflow = -halfLoop - segLeft;
                        SpawnOverflowGhost(segment, overflow, true, halfLoop);
                    }

                    if (segRight > halfLoop) {
                        float overflow = segRight - halfLoop;
                        SpawnOverflowGhost(segment, overflow, false, halfLoop);
                    }
                }
                else {
                    segment.SetWidth(_segmentHeight);
                    segment.SetVisibleSpan(-halfSpan, halfSpan);
                    segment.SetVisible(true);
                }
            }
        }

        private void SpawnOverflowGhost(StripSegment source, float overflow, bool atRightEdge, float halfLoop) {
            float ghostCenterX = atRightEdge
                ? halfLoop - overflow * 0.5f
                : -halfLoop + overflow * 0.5f;
            float ghostHalfSpan = overflow * 0.5f;

            float ghostLeft = Mathf.Max(ghostCenterX - ghostHalfSpan, -halfLoop);
            float ghostRight = Mathf.Min(ghostCenterX + ghostHalfSpan, halfLoop);

            if (ghostLeft >= ghostRight)
                return;

            StripSegment ghost = Instantiate(source, transform);
            ghost.gameObject.name = source.gameObject.name + "_WrapGhost";
            ghost.SetConfig(source.GetConfig().Clone());
            ghost.SetCenterX(ghostCenterX);
            ghost.SetWidth(_segmentHeight, true);
            ghost.SetVisibleSpan(ghostLeft - ghostCenterX, ghostRight - ghostCenterX);
            ghost.SetVisible(true);
            ghost.SetSortingOrder(source.GetSortingOrder() - 1);
            ghost.HideStatusIcon();
            _wrapGhosts.Add(ghost);
        }

        private static float WrapHorizontal(float value, float loopLength) {
            return Mathf.Repeat(value, loopLength);
        }

        private static int Mod(int value, int count) {
            if (count <= 0) return 0;
            int result = value % count;
            return result < 0 ? result + count : result;
        }

        private void ClearStrip() {
            ClearWrapGhosts();

            foreach (StripSegment seg in _spawnedSegments) {
                if (seg != null)
                    DestroyImmediate(seg.gameObject);
            }

            _spawnedSegments.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--) {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}
