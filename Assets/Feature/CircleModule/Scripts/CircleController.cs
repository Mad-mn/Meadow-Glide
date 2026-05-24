using System.Collections.Generic;
using Feature.ColorServiceModule.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleController : MonoBehaviour {
        [SerializeField] private CircleSegment _segmentPrefab;

        private readonly List<CircleSegment> _spawnedSegments = new List<CircleSegment>();

        private CircleConfig _currentConfig;
        private ICircleColorService _circleColorService;
        private float _calculatedRadius;

        public float Radius => _calculatedRadius;
        public int SegmentCount => _currentConfig != null ? _currentConfig.segmentCount : 0;

        [Inject]
        public void InjectDependencies(ICircleColorService colorService) {
            _circleColorService = colorService;
        }

        public void Setup(CircleConfig config, float radius) {
            _currentConfig = config;
            _calculatedRadius = radius;
            BuildCircle();
        }

        private void BuildCircle() {
            ClearCircle();

            if (_currentConfig == null || _segmentPrefab == null || _circleColorService == null) {
                Debug.LogWarning("CircleController: Missing references or config!");
                return;
            }

            float anglePerSegment = 360f / _currentConfig.segmentCount;

            for (int i = 0; i < _currentConfig.segmentCount; i++) {
                SegmentConfig segData;
                if (_currentConfig.segments != null && i < _currentConfig.segments.Count) {
                    // Create a copy to avoid modifying the ScriptableObject data if we change radius
                    var originalSeg = _currentConfig.segments[i];
                    segData = new SegmentConfig {
                        colorType = originalSeg.colorType,
                        radius = _calculatedRadius, // Force calculated radius
                        angle = anglePerSegment
                    };
                }
                else {
                    // Fallback to default segment data
                    segData = new SegmentConfig {
                        radius = _calculatedRadius, 
                        angle = anglePerSegment, 
                        colorType = CircleColorType.White
                    };
                }

                float rotationAngle = i * anglePerSegment;

                CircleSegment segment = Instantiate(_segmentPrefab, transform);
                segment.transform.localRotation = Quaternion.Euler(0, 0, rotationAngle);

                Color color = _circleColorService.GetColor(segData.colorType);
                segment.Initialize(segData, color);

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