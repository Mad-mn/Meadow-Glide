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

        [Inject]
        public void InjectDependencies(ICircleColorService colorService) {
            _circleColorService = colorService;
        }

        public void Setup(CircleConfig config) {
            _currentConfig = config;
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
                    segData = _currentConfig.segments[i];
                }
                else {
                    // Fallback to default segment data based on circle config
                    segData = new SegmentConfig {
                        radius = _currentConfig.radius, angle = anglePerSegment, colorType = CircleColorType.White
                    };
                }

                if (segData.radius <= 0)
                    segData.radius = _currentConfig.radius;

                segData.angle = anglePerSegment;

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