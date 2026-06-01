using System;
using System.Collections.Generic;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleController : MonoBehaviour {
        [SerializeField] private CircleSegment _segmentPrefab;

        private readonly List<CircleSegment> _spawnedSegments = new List<CircleSegment>();

        private CircleConfig _currentConfig;
        private ICircleColorService _circleColorService;
        private ISegmentStatusVisualDataProvider _statusVisualDataProvider;
        private float _calculatedRadius;
        private float _segmentWidth;

        public float Radius => _calculatedRadius;
        public int SegmentCount => _currentConfig != null ? _currentConfig.SegmentCount : 0;

        public IReadOnlyList<CircleSegment> SpawnedSegments => _spawnedSegments;

        [Inject]
        public void InjectDependencies(ICircleColorService colorService, ISegmentStatusVisualDataProvider statusVisualDataProvider, GameCircleModel circleModel) {
            _circleColorService = colorService;
            _statusVisualDataProvider = statusVisualDataProvider;
        }

        public CircleSegment GetSegmentAtAngle(float worldAngle) {
            if (_spawnedSegments.Count == 0) return null;
            
            float anglePerSegment = 360f / _currentConfig.SegmentCount;
            // Normalize world angle to [0, 360)
            float normalizedWorldAngle = (worldAngle % 360 + 360) % 360;
            
            CircleSegment bestSeg = null;
            float minDelta = float.MaxValue;

            foreach (var seg in _spawnedSegments) {
                // World rotation of the segment
                float segWorldAngle = (transform.eulerAngles.z + seg.transform.localEulerAngles.z) % 360;
                segWorldAngle = (segWorldAngle + 360) % 360;
                
                float delta = Mathf.Abs(Mathf.DeltaAngle(segWorldAngle, normalizedWorldAngle));
                if (delta < minDelta) {
                    minDelta = delta;
                    bestSeg = seg;
                }
            }
            
            if (minDelta < anglePerSegment / 2f) {
                return bestSeg;
            }
            return null;
        }

        public void RemoveSegment(CircleSegment segment) {
            _spawnedSegments.Remove(segment);
        }

        public void AddSegment(CircleSegment segment) {
            segment.transform.SetParent(transform);
            _spawnedSegments.Add(segment);
            segment.SetWidth(_segmentWidth);
        }

        public void Setup(CircleConfig config, float radius, float width) {
            _currentConfig = config;
            _calculatedRadius = radius;
            _segmentWidth = width;
            BuildCircle();
        }

        private void BuildCircle() {
            ClearCircle();

            if (_currentConfig == null || _segmentPrefab == null || _circleColorService == null) {
                Debug.LogWarning("CircleController: Missing references or config!");
                return;
            }

            float anglePerSegment = 360f / _currentConfig.SegmentCount;

            for (int i = 0; i < _currentConfig.SegmentCount; i++) {
                SegmentConfig segData;
                if (_currentConfig.Segments != null && i < _currentConfig.Segments.Count) {
                    // Create a copy to avoid modifying the ScriptableObject data if we change radius
                    var originalSeg = _currentConfig.Segments[i];
                    segData = new SegmentConfig {
                        ColorType = originalSeg.ColorType,
                        Radius = _calculatedRadius, // Force calculated radius
                        Angle = anglePerSegment,
                        SegmentStatus = originalSeg.SegmentStatus
                    };
                }
                else {
                    // Fallback to default segment data
                    segData = new SegmentConfig {
                        Radius = _calculatedRadius, 
                        Angle = anglePerSegment, 
                        ColorType = CircleColorType.None,
                        SegmentStatus = SegmentStatus.Default
                    };
                }

                float rotationAngle = i * anglePerSegment;

                CircleSegment segment = Instantiate(_segmentPrefab, transform);
                segment.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);

                Color color = _circleColorService.GetColor(segData.ColorType);
                segment.Initialize(segData, color, _segmentWidth, _statusVisualDataProvider);

                _spawnedSegments.Add(segment);
            }
        }

        private void ClearCircle() {
            foreach (var seg in _spawnedSegments) {
                if (seg != null)
                    DestroyImmediate(seg.gameObject);
            }

            _spawnedSegments.Clear();

            // Also clean up any orphan children
            for (int i = transform.childCount - 1; i >= 0; i--) {
                DestroyImmediate(transform.GetChild(i)
                    .gameObject);
            }
        }
    }
}