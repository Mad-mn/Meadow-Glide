using Cysharp.Threading.Tasks.Triggers;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using UnityEngine;

namespace Feature.CircleModule.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class CircleSegment : MonoBehaviour
    {
        [SerializeField] private GameObject _trigger;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private SpriteRenderer _statusIcon;
        [SerializeField] private SegmentStatusAnimator _statusAnimator;
        [SerializeField] private float _weightCoefficient = 0.5f;
        [SerializeField] private float _zoomScaleMultiplier = 2;

        private const int SegmentsPerArc = 40;
        private SegmentConfig _currentConfig;
        private ISegmentStatusVisualDataProvider _visualDataProvider;
        private float _currentRadius;
        private float _currentWidth;

        private LineRenderer _cachedLineRenderer;
        private Vector3[] _unitArcPositions;
        private float _lastPrecalculatedAngle = -1f;
        private bool _zoomed;

        public float Radius => _currentConfig != null ? _currentConfig.Radius : 0;
        public CircleColorType ColorType => _currentConfig != null ? _currentConfig.ColorType : CircleColorType.None;

        public bool IsBlocked => _currentConfig != null && _currentConfig.SegmentStatus == SegmentStatus.Blocked;

        public void Initialize(SegmentConfig config, Color color, float width, ISegmentStatusVisualDataProvider visualDataProvider)
        {
            _currentConfig = config;
            _visualDataProvider = visualDataProvider;
            _currentRadius = config.Radius;
            _currentWidth = width;
            
            EnsureLineRenderer();
            
            if (_statusAnimator == null)
                _statusAnimator = GetComponentInChildren<SegmentStatusAnimator>();
            
            if (_statusAnimator != null && _statusIcon != null)
                _statusAnimator.SetTarget(_statusIcon.transform);

            // Set visuals
            _cachedLineRenderer.useWorldSpace = false;
            _cachedLineRenderer.startColor = color;
            _cachedLineRenderer.endColor = color;
            _cachedLineRenderer.startWidth = width;
            _cachedLineRenderer.endWidth = width;
            
            DrawArc(_currentRadius, config.Angle);

            SetupTriggerPosition(config);
            UpdateStatusIcon();
        }

        public SegmentConfig GetConfig() => _currentConfig;

        public void SetConfig(SegmentConfig config)
        {
            _currentConfig = config;
            _currentRadius = config.Radius;
            UpdateStatusIcon();
        }

        public void TriggerBlockedAnimation()
        {
            if (_statusAnimator != null)
                _statusAnimator.PlayShake().Forget();
        }

        public void SetWidth(float width) {
            _currentWidth = width;
            EnsureLineRenderer();
            _cachedLineRenderer.startWidth = width;
            _cachedLineRenderer.endWidth = width;
            UpdateStatusIcon();
        }

        public void SetStatus(SegmentStatus status)
        {
            if (_currentConfig == null) return;
            _currentConfig.SegmentStatus = status;
            UpdateStatusIcon();
        }

        public void HideStatusIcon() {
            _statusIcon.gameObject.SetActive(false);
        }

        public SegmentStatus GetStatus() {
            if (_currentConfig == null) return SegmentStatus.Default;
            return _currentConfig.SegmentStatus;
        }

        public void SetVisible(bool visible) {
            EnsureLineRenderer();
            _cachedLineRenderer.enabled = visible;
            if (_trigger != null) _trigger.SetActive(visible);
            _statusIcon.gameObject.SetActive(visible);
        }

        public int GetSortingOrder() {
            EnsureLineRenderer();
            return _cachedLineRenderer.sortingOrder;
        }

        public void SetSortingOrder(int order) {
            EnsureLineRenderer();
            _cachedLineRenderer.sortingOrder = order;
        }

        public void SetRadius(float radius) {
            _currentRadius = radius;
            if (_currentConfig != null) _currentConfig.Radius = radius;
            
            DrawArc(radius, _currentConfig != null ? _currentConfig.Angle : 360f / 4f);
            if (_currentConfig != null) SetupTriggerPosition(_currentConfig);
            UpdateStatusIcon();
        }

        public void ZoomIn() {
            if (_zoomed)
                return;

            Debug.LogError(3);
            SetWidth(_currentWidth * _zoomScaleMultiplier);
            _zoomed = true;
        }

        public void ZoomOut() {
            if (!_zoomed)
                return;
            
            SetWidth(_currentWidth / _zoomScaleMultiplier);
            _zoomed = false;
        }

        private void UpdateStatusIcon()
        {
            if (_statusIcon == null || _visualDataProvider == null) return;
            
            EnsureLineRenderer();
            
            bool shouldBeVisible = _currentConfig != null && 
                                   _currentConfig.SegmentStatus != SegmentStatus.Default && 
                                   _cachedLineRenderer.enabled && 
                                   _currentWidth > 0.001f;

            if (!shouldBeVisible)
            {
                _statusIcon.gameObject.SetActive(false);
                return;
            }

            var visualData = _visualDataProvider.GetVisualDataByStatus(_currentConfig.SegmentStatus);
            if (visualData == null)
            {
                _statusIcon.gameObject.SetActive(false);
               return;
            }

            _statusIcon.gameObject.SetActive(true);
            _statusIcon.sprite = visualData.StatusIcon;
            
            // Position at the middle of the arc
            _statusIcon.transform.localPosition = new Vector3(_currentRadius, 0, 0);
            
            // Rotation: bottom to center
            _statusIcon.transform.localRotation = Quaternion.Euler(0, 0, -90f);
            
            // Scale: width * WightCoeffiecient
            float targetSize = _currentWidth * visualData.WightCoeffiecient;
            if (_statusIcon.sprite != null)
            {
                float spriteSize = _statusIcon.sprite.bounds.size.x;
                if (spriteSize > 0)
                {
                    float s = targetSize / spriteSize;
                    _statusIcon.transform.localScale = new Vector3(s, s, 1f);
                }
            }
            else
            {
                _statusIcon.transform.localScale = new Vector3(targetSize, targetSize, 1f);
            }
        }

        private void EnsureLineRenderer() {
            if (_cachedLineRenderer == null) {
                _cachedLineRenderer = _lineRenderer != null ? _lineRenderer : GetComponent<LineRenderer>();
            }
        }

        private void SetupTriggerPosition(SegmentConfig config) {
            if (_trigger != null)
            {
                _trigger.transform.localPosition = new Vector3(config.Radius, 0, 0);
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
