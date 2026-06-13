using Feature.CircleModule.Scripts;
using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using UnityEngine;

namespace Feature.StripsModule.Scripts {
    [RequireComponent(typeof(LineRenderer))]
    public class StripSegment : MonoBehaviour, IGameSegment {
        [SerializeField] private GameObject _trigger;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private SpriteRenderer _statusIcon;
        [SerializeField] private SegmentAnimationController _animationController;
        [SerializeField] private float _zoomScaleMultiplier = 2f;

        private SegmentConfig _currentConfig;
        private ISegmentStatusVisualDataProvider _visualDataProvider;
        private float _centerY;
        private float _halfSpan;
        private float _currentHeight;
        private bool _zoomed;

        public float Radius => _centerY;
        public CircleColorType ColorType => _currentConfig != null ? _currentConfig.ColorType : CircleColorType.None;
        public bool IsBlocked => _currentConfig != null && _currentConfig.SegmentStatus == SegmentStatus.Blocked;
        public float CurrentWight => _currentHeight;

        public void Initialize(SegmentConfig config, Color color, float height, float halfSpan,
            ISegmentStatusVisualDataProvider visualDataProvider) {
            _currentConfig = config;
            _visualDataProvider = visualDataProvider;
            _centerY = config.Radius;
            _currentHeight = height;
            _halfSpan = halfSpan;

            _lineRenderer.useWorldSpace = false;
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            _lineRenderer.startWidth = height;
            _lineRenderer.endWidth = height;

            DrawHorizontalLine();
            SetupTriggerPosition();
            UpdateStatusIcon();
        }

        public SegmentConfig GetConfig() => _currentConfig;

        public void SetConfig(SegmentConfig config) {
            _currentConfig = config;
            _centerY = config.Radius;
            UpdateStatusIcon();
        }

        public void TriggerBlockedAnimation() {
            if (_animationController != null)
                _animationController.TriggerBlockedAnimation();
        }

        public void SetWidth(float height, bool zoomed = false) {
            _currentHeight = zoomed ? height * _zoomScaleMultiplier : height;
            _lineRenderer.startWidth = _currentHeight;
            _lineRenderer.endWidth = _currentHeight;
            UpdateStatusIcon();
        }

        public void SetSpan(float halfSpan) {
            _halfSpan = halfSpan;
            DrawHorizontalLine();
            SetupTriggerPosition();
            UpdateStatusIcon();
        }

        public void SetVisibleSpan(float localLeftX, float localRightX) {
            _lineRenderer.SetPosition(0, new Vector3(localLeftX, 0, 0));
            _lineRenderer.SetPosition(1, new Vector3(localRightX, 0, 0));
        }

        public void SetStatus(SegmentStatus status) {
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
            UpdateStatusIcon();
        }

        public int GetSortingOrder() => _lineRenderer.sortingOrder;

        public void SetSortingOrder(int order) {
            _lineRenderer.sortingOrder = order;
        }

        public void SetRadius(float y) {
            _centerY = y;
            if (_currentConfig != null) _currentConfig.Radius = y;
            transform.localPosition = new Vector3(transform.localPosition.x, y, 0);
            SetupTriggerPosition();
            UpdateStatusIcon();
        }

        public void SetCenterX(float x) {
            transform.localPosition = new Vector3(x, _centerY, 0);
            SetupTriggerPosition();
        }

        public void ZoomIn(bool force = false) {
            if (_zoomed && !force)
                return;

            _lineRenderer.sortingOrder *= 2;
            SetWidth(_currentHeight * _zoomScaleMultiplier);
            _zoomed = true;
        }

        public void ZoomOut() {
            if (!_zoomed)
                return;

            _lineRenderer.sortingOrder /= 2;
            SetWidth(_currentHeight / _zoomScaleMultiplier);
            _zoomed = false;
        }

        private void DrawHorizontalLine() {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, new Vector3(-_halfSpan, 0, 0));
            _lineRenderer.SetPosition(1, new Vector3(_halfSpan, 0, 0));
        }

        private void SetupTriggerPosition() {
            if (_trigger != null)
                _trigger.transform.localPosition = Vector3.zero;
        }

        private void UpdateStatusIcon() {
            if (_statusIcon == null || _visualDataProvider == null) return;

            bool shouldBeVisible = _currentConfig != null &&
                                   _currentConfig.SegmentStatus != SegmentStatus.Default &&
                                   _lineRenderer.enabled &&
                                   _currentHeight > 0.001f;

            if (!shouldBeVisible) {
                _statusIcon.gameObject.SetActive(false);
                return;
            }

            var visualData = _visualDataProvider.GetVisualDataByStatus(_currentConfig.SegmentStatus);
            if (visualData == null) {
                _statusIcon.gameObject.SetActive(false);
                return;
            }

            _statusIcon.gameObject.SetActive(true);
            _statusIcon.sprite = visualData.StatusIcon;
            _statusIcon.transform.localPosition = Vector3.zero;
            _statusIcon.transform.localRotation = Quaternion.identity;

            float targetSize = _currentHeight * visualData.WightCoeffiecient;
            if (_statusIcon.sprite != null) {
                float spriteSize = _statusIcon.sprite.bounds.size.x;
                if (spriteSize > 0) {
                    float s = targetSize / spriteSize;
                    _statusIcon.transform.localScale = new Vector3(s, s, 1f);
                }
            }
            else {
                _statusIcon.transform.localScale = new Vector3(targetSize, targetSize, 1f);
            }
        }
    }
}
