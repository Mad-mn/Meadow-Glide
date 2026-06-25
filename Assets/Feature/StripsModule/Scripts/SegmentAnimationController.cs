using DG.Tweening;
using Feature.CircleModule.Scripts;
using UnityEngine;

namespace Feature.StripsModule.Scripts {
    public class SegmentAnimationController : MonoBehaviour {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private float _zoomMultiplier = 2f;
        [SerializeField] private float _zoomInDuration = 0.2f;
        [SerializeField] private float _zoomOutDuration = 0.2f;
        [SerializeField] private Ease _zoomInEase = Ease.OutBack;
        [SerializeField] private Ease _zoomOutEase = Ease.InOutQuad;
        [SerializeField] private SegmentStatusAnimator _statusAnimator;

        private float _baseHeight;
        private float _currentHeight;
        private int _baseSortingOrder;
        private Sequence _zoomSequence;
        private bool _isAnimating;

        public bool IsAnimating => _isAnimating;
        public bool IsZoomed { get; private set; }
        public float ZoomedHeight => _baseHeight * _zoomMultiplier;

        public void Initialize(float height, int sortingOrder) {
            _baseHeight = height;
            _currentHeight = height;
            _baseSortingOrder = sortingOrder;
        }

        private bool _zoomed;

        public void TriggerBlockedAnimation()
        {
            if (_statusAnimator != null)
                _statusAnimator.PlayShake().Forget();
        }

        public void ZoomIn(bool force = false) {
            if (IsZoomed && !force)
                return;
            if (_isAnimating && !force)
                return;

            IsZoomed = true;
            _isAnimating = true;

            _zoomSequence?.Kill();
            _zoomSequence = DOTween.Sequence();
            _zoomSequence.Append(DOTween.To(() => _currentHeight, x => {
                _currentHeight = x;
                _lineRenderer.startWidth = x;
                _lineRenderer.endWidth = x;
            }, _baseHeight * _zoomMultiplier, _zoomInDuration).SetEase(_zoomInEase));
            _zoomSequence.OnStart(() => _lineRenderer.sortingOrder = _baseSortingOrder * 2);
            _zoomSequence.OnComplete(() => _isAnimating = false);
        }

        public void ZoomOut() {
            if (!IsZoomed)
                return;

            IsZoomed = false;
            _isAnimating = true;

            _zoomSequence?.Kill();
            _zoomSequence = DOTween.Sequence();
            _zoomSequence.Append(DOTween.To(() => _currentHeight, x => {
                _currentHeight = x;
                _lineRenderer.startWidth = x;
                _lineRenderer.endWidth = x;
            }, _baseHeight, _zoomOutDuration).SetEase(_zoomOutEase));
            _zoomSequence.OnComplete(() => {
                _lineRenderer.sortingOrder = _baseSortingOrder;
                _isAnimating = false;
            });
        }

        public void ForceResetZoom() {
            _zoomSequence?.Kill();
            IsZoomed = false;
            _isAnimating = false;
            _currentHeight = _baseHeight;
            _lineRenderer.startWidth = _baseHeight;
            _lineRenderer.endWidth = _baseHeight;
            _lineRenderer.sortingOrder = _baseSortingOrder;
        }

        public void SetBaseHeight(float height) {
            _baseHeight = height;
            if (!IsZoomed && !_isAnimating) {
                _currentHeight = height;
                _lineRenderer.startWidth = height;
                _lineRenderer.endWidth = height;
            }
        }

        public void SetBaseSortingOrder(int order) {
            _baseSortingOrder = order;
        }

        private void OnDestroy() {
            _zoomSequence?.Kill();
        }
    }
}
