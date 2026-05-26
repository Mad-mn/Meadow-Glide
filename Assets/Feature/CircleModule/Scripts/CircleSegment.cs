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

        public float Radius => _currentConfig != null ? _currentConfig.radius : 0;

        public void Initialize(SegmentConfig config, Color color)
        {
            _currentConfig = config;
            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
            
            // Set visuals
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            _lineRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            
            DrawArc(config.radius, config.angle);

            SetupTriggerPosition(config);
        }

        public void SetRadius(float radius) {
            if (_currentConfig == null) return;
            _currentConfig.radius = radius;
            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
            DrawArc(radius, _currentConfig.angle);
            SetupTriggerPosition(_currentConfig);
        }

        private void SetupTriggerPosition(SegmentConfig config) {
            if (_trigger != null)
            {
                _trigger.transform.localPosition = new Vector3(config.radius, 0, 0);
            }
        }

        private void DrawArc(float radius, float angle)
        {
            _lineRenderer.positionCount = SegmentsPerArc + 1;
            
            for (int i = 0; i <= SegmentsPerArc; i++)
            {
                float progress = (float)i / SegmentsPerArc;
                // Center the arc around the zero point of rotation
                float currentAngle = (progress * angle - angle / 2f) * Mathf.Deg2Rad;
                
                float x = Mathf.Cos(currentAngle) * radius;
                float y = Mathf.Sin(currentAngle) * radius;
                
                _lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
    }
}
