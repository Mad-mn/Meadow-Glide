using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Feature.StripsModule.Scripts {
    public class StripController : MonoBehaviour {
        [SerializeField] private CircleSegment _segmentPrefab;
        [SerializeField] private StripAnimationController _stripAnimationController;

        private readonly List<CircleSegment> _spawnedSegments = new List<CircleSegment>();

        private CircleConfig _currentConfig;
        private ICircleColorService _circleColorService;
        private ISegmentStatusVisualDataProvider _statusVisualDataProvider;
        private float _segmentWidth;
        private int _positionIndex;
        
        public bool IsCompleted {
            get {
                CircleColorType circleColorType = CircleColorType.None;
                bool completed = true;
                foreach (CircleSegment circleSegment in _spawnedSegments) {
                    if (circleColorType is CircleColorType.None) {
                        circleColorType = circleSegment.ColorType;
                        continue;
                    }

                    if (circleSegment.ColorType != circleColorType) {
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

        public IReadOnlyList<CircleSegment> SpawnedSegments =>
            _spawnedSegments;

        [Inject]
        public void InjectDependencies(ICircleColorService colorService, ISegmentStatusVisualDataProvider statusVisualDataProvider,
            GameCircleModel circleModel) {
            _circleColorService = colorService;
            _statusVisualDataProvider = statusVisualDataProvider;
        }

        public void Setup(CircleConfig config, float width, int positionIndex) {
            _currentConfig = config;
            _segmentWidth = width;
            _positionIndex = positionIndex;
            BuildStrip();
        }

        private void BuildStrip() {
            ClearStrip();

            if (_currentConfig == null || _segmentPrefab == null || _circleColorService == null) {
                Debug.LogError("StripController: Missing references or config!");
                return;
            }

            for (int i = 0; i < _currentConfig.SegmentCount; i++) {
                SegmentConfig segData;
                if (_currentConfig.Segments != null && i < _currentConfig.Segments.Count) {
                    var originalSeg = _currentConfig.Segments[i];
                    segData = new SegmentConfig {
                        ColorType = originalSeg.ColorType, SegmentStatus = originalSeg.SegmentStatus
                    };
                }
                else {
                    segData = new SegmentConfig {
                        ColorType = CircleColorType.None, SegmentStatus = SegmentStatus.Default
                    };
                }

                CircleSegment segment = Instantiate(_segmentPrefab, transform);

                Color color = _circleColorService.GetColor(segData.ColorType);
                segment.Initialize(segData, color, _segmentWidth, _statusVisualDataProvider);

                _spawnedSegments.Add(segment);
            }
        }

        public void RemoveSegment(CircleSegment segment) {
            _spawnedSegments.Remove(segment);
        }

        public void AddSegment(CircleSegment segment) {
            segment.transform.SetParent(transform);
            _spawnedSegments.Add(segment);
            segment.SetWidth(_segmentWidth);
        }

        private void ClearStrip() {
            foreach (var seg in _spawnedSegments) {
                if (seg != null)
                    DestroyImmediate(seg.gameObject);
            }

            _spawnedSegments.Clear();

            for (int i = transform.childCount - 1; i >= 0; i--) {
                DestroyImmediate(transform.GetChild(i)
                    .gameObject);
            }
        }

        public void PlayCompletedAnimation(Action callback) {
            _stripAnimationController.PlayCompletedAnimation(callback);
        }
    }
}