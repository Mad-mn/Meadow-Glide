using Cysharp.Threading.Tasks.Triggers;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using UnityEngine;
using UnityEngine.Serialization;

namespace Feature.CircleModule.Scripts
{
    [RequireComponent(typeof(LineRenderer))]
    public class CircleSegment : MonoBehaviour, IGameSegment
    {
        [SerializeField] private GameObject _trigger;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private SpriteRenderer _statusIcon;
        [SerializeField] private SegmentAnimationController _animationController;
        [SerializeField] private float _weightCoefficient = 0.5f;
        [SerializeField] private float _zoomScaleMultiplier = 2;

        private const int SegmentsPerArc = 40;
        private SegmentConfig _currentConfig;
        private ISegmentStatusVisualDataProvider _visualDataProvider;
        private float _currentRadius;
        private float _currentWidth;

        private Vector3[] _unitArcPositions;
        private float _lastPrecalculatedAngle = -1f;
        private bool _zoomed;

        public float Radius => _currentConfig != null ? _currentConfig.Radius : 0;
        public CircleColorType ColorType => _currentConfig != null ? _currentConfig.ColorType : CircleColorType.None;

        public bool IsBlocked => _currentConfig != null && _currentConfig.SegmentStatus == SegmentStatus.Blocked;
        public float CurrentWight =>
            _currentWidth;

        public void Initialize(SegmentConfig config, Color color, float width, ISegmentStatusVisualDataProvider visualDataProvider)
        {
            _currentConfig = config;
            _visualDataProvider = visualDataProvider;
            _currentRadius = config.Radius;
            _currentWidth = width;
            
            _lineRenderer.useWorldSpace = false;
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            
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
            if (_animationController != null)
                _animationController.TriggerBlockedAnimation();
        }

        public void SetWidth(float width, bool zommed = false) {
            _currentWidth = zommed ? width * _zoomScaleMultiplier : width;
            _lineRenderer.startWidth = zommed ? width * _zoomScaleMultiplier : width;
            _lineRenderer.endWidth = zommed ? width * _zoomScaleMultiplier : width;
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
            _lineRenderer.enabled = visible;
            if (_trigger != null) _trigger.SetActive(visible);
            _statusIcon.gameObject.SetActive(visible);
        }

        public int GetSortingOrder() {
            return _lineRenderer.sortingOrder;
        }

        public void SetSortingOrder(int order) {
            _lineRenderer.sortingOrder = order;
        }

        public void SetRadius(float radius) {
            _currentRadius = radius;
            if (_currentConfig != null) _currentConfig.Radius = radius;
            
            DrawArc(radius, _currentConfig != null ? _currentConfig.Angle : 360f / 4f);
            if (_currentConfig != null) SetupTriggerPosition(_currentConfig);
            UpdateStatusIcon();
        }

        public void ZoomIn(bool force = false) {
            if (_zoomed && !force)
                return;

            _lineRenderer.sortingOrder *= 2;
            SetWidth(_currentWidth * _zoomScaleMultiplier);
            _zoomed = true;
        }

        public void ZoomOut() {
            if (!_zoomed)
                return;

            _lineRenderer.sortingOrder /= 2;
            SetWidth(_currentWidth / _zoomScaleMultiplier);
            _zoomed = false;
        }

        private void UpdateStatusIcon()
        {
            if (_statusIcon == null || _visualDataProvider == null) return;
            
            bool shouldBeVisible = _currentConfig != null && 
                                   _currentConfig.SegmentStatus != SegmentStatus.Default && 
                                   _lineRenderer.enabled && 
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
            
            _statusIcon.transform.localPosition = new Vector3(_currentRadius, 0, 0);
            
            _statusIcon.transform.localRotation = Quaternion.Euler(0, 0, -90f);
            
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

        private void SetupTriggerPosition(SegmentConfig config) {
            if (_trigger != null)
            {
                _trigger.transform.localPosition = new Vector3(config.Radius, 0, 0);
            }
        }

        private void DrawArc(float radius, float angle)
        {
            _lineRenderer.positionCount = SegmentsPerArc + 1;

            if (Mathf.Abs(_lastPrecalculatedAngle - angle) > 0.001f) {
                PrecalculateUnitArc(angle);
            }
            
            for (int i = 0; i <= SegmentsPerArc; i++)
            {
                _lineRenderer.SetPosition(i, _unitArcPositions[i] * radius);
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
