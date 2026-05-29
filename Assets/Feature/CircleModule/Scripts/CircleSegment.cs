using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace Feature.CircleModule.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class CircleSegment : MonoBehaviour
    {
        [SerializeField] private GameObject _trigger;
        [SerializeField] private LineRenderer _lineRenderer;
        
        private const int SegmentsPerArc = 40;
        [SerializeField] private SegmentConfig _currentConfig;

        private LineRenderer _cachedLineRenderer;
        private Vector3[] _unitArcPositions;
        private float _lastPrecalculatedAngle = -1f;

        public float Radius => _currentConfig != null ? _currentConfig.radius : 0;

        public void Initialize(SegmentConfig config, Color color, float width)
        {
            _currentConfig = config;
            EnsureLineRenderer();
            
            // Set visuals
            _cachedLineRenderer.useWorldSpace = false;
            _cachedLineRenderer.startColor = color;
            _cachedLineRenderer.endColor = color;
            _cachedLineRenderer.startWidth = width;
            _cachedLineRenderer.endWidth = width;
            _cachedLineRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            
            DrawArc(config.radius, config.angle);

            SetupTriggerPosition(config);
        }

        public void SetWidth(float width) {
            EnsureLineRenderer();
            _cachedLineRenderer.startWidth = width;
            _cachedLineRenderer.endWidth = width;
        }

        private void EnsureLineRenderer() {
            if (_cachedLineRenderer == null) {
                _cachedLineRenderer = _lineRenderer != null ? _lineRenderer : GetComponent<LineRenderer>();
            }
        }

        public void SetRadius(float radius) {
            if (_currentConfig == null) return;
            _currentConfig.radius = radius;
            DrawArc(radius, _currentConfig.angle);
            SetupTriggerPosition(_currentConfig);
        }

        public void SetVisible(bool visible) {
            EnsureLineRenderer();
            _cachedLineRenderer.enabled = visible;
            if (_trigger != null) _trigger.SetActive(visible);
        }

        private void SetupTriggerPosition(SegmentConfig config) {
            if (_trigger != null)
            {
                _trigger.transform.localPosition = new Vector3(config.radius, 0, 0);
            }
        }

        private void DrawArc(float radius, float angle)
        {
            EnsureLineRenderer();
            _cachedLineRenderer.positionCount = SegmentsPerArc + 1;

            if (Mathf.Abs(_lastPrecalculatedAngle - angle) > 0.001f) {
                PrecalculateUnitArc(angle);
            }
            
            for (int i = 0; i <= SegmentsPerArc; i++)
            {
                _cachedLineRenderer.SetPosition(i, _unitArcPositions[i] * radius);
            }
        }

        private void PrecalculateUnitArc(float angle) {
            _unitArcPositions = new Vector3[SegmentsPerArc + 1];
            for (int i = 0; i <= SegmentsPerArc; i++) {
                float progress = (float)i / SegmentsPerArc;
                float currentAngle = (progress * angle - angle / 2f) * Mathf.Deg2Rad;
                _unitArcPositions[i] = new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0);
            }
            _lastPrecalculatedAngle = angle;
        }
    }
}
