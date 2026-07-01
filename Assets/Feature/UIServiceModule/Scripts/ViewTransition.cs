using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Feature.UIServiceModule.Scripts {
    [DisallowMultipleComponent]
    public class ViewTransition : MonoBehaviour {
        [Header("Target")]
        [Tooltip("Об'єкт який анімується. Якщо null — анімується весь gameObject.")]
        [SerializeField] private RectTransform _animationTarget;

        [Header("Show Animation")]
        [SerializeField] private bool _animateShow = true;
        [SerializeField] private float _showDuration = 0.25f;
        [SerializeField] private Ease _showEase = Ease.OutQuad;
        [SerializeField] private float _showScaleFrom = 0.92f;
        [SerializeField] private float _showSlideOffsetY = 40f;

        [Header("Hide Animation")]
        [SerializeField] private bool _animateHide = true;
        [SerializeField] private float _hideDuration = 0.2f;
        [SerializeField] private Ease _hideEase = Ease.InQuad;
        [SerializeField] private float _hideScaleTo = 0.95f;
        [SerializeField] private float _hideSlideOffsetY = 30f;

        public event Action Shown;
        public event Action Hidden;

        private CanvasGroup _canvasGroup;
        private RectTransform _targetRect;
        private Vector3 _originalScale;
        private Vector2 _originalAnchoredPosition;
        private Sequence _activeSequence;

        public bool IsAnimating => _activeSequence != null && _activeSequence.IsActive();

        private void Awake() {
            _targetRect = _animationTarget != null ? _animationTarget : GetComponent<RectTransform>();
            _canvasGroup = _animationTarget != null
                ? GetOrAddCanvasGroup(_animationTarget)
                : GetOrAddCanvasGroup(transform);
            _originalScale = _targetRect.localScale;
            _originalAnchoredPosition = _targetRect.anchoredPosition;
        }

        public UniTask PlayShow() {
            if (!_animateShow) {
                Shown?.Invoke();
                return UniTask.CompletedTask;
            }

            KillActive();
            SetInitialState();

            var tcs = new UniTaskCompletionSource();

            _activeSequence = DOTween.Sequence();
            _activeSequence.Append(_targetRect.DOScale(_showScaleFrom, 0f));
            _activeSequence.Append(_targetRect.DOScale(Vector3.one, _showDuration).SetEase(_showEase));
            _activeSequence.Join(_targetRect.DOAnchorPosY(_originalAnchoredPosition.y + _showSlideOffsetY, 0f));
            _activeSequence.Join(_targetRect.DOAnchorPosY(_originalAnchoredPosition.y, _showDuration).SetEase(_showEase));
            _activeSequence.Join(_canvasGroup.DOFade(0f, 0f));
            _activeSequence.Join(_canvasGroup.DOFade(1f, _showDuration).SetEase(_showEase));
            _activeSequence.OnComplete(() => {
                _activeSequence = null;
                Shown?.Invoke();
                tcs.TrySetResult();
            });

            return tcs.Task;
        }

        public UniTask PlayHide() {
            if (!_animateHide) {
                RestoreState();
                Hidden?.Invoke();
                return UniTask.CompletedTask;
            }

            KillActive();

            var tcs = new UniTaskCompletionSource();

            _activeSequence = DOTween.Sequence();
            _activeSequence.Append(_targetRect.DOScale(_hideScaleTo, _hideDuration).SetEase(_hideEase));
            _activeSequence.Join(_targetRect.DOAnchorPosY(_originalAnchoredPosition.y - _hideSlideOffsetY, _hideDuration).SetEase(_hideEase));
            _activeSequence.Join(_canvasGroup.DOFade(0f, _hideDuration).SetEase(_hideEase));
            _activeSequence.OnComplete(() => {
                _activeSequence = null;
                RestoreState();
                Hidden?.Invoke();
                tcs.TrySetResult();
            });

            return tcs.Task;
        }

        public void KillActive() {
            if (_activeSequence != null && _activeSequence.IsActive()) {
                _activeSequence.Kill(false);
                _activeSequence = null;
            }
        }

        private void SetInitialState() {
            _targetRect.localScale = _originalScale * _showScaleFrom;
            _targetRect.anchoredPosition = _originalAnchoredPosition + new Vector2(0, _showSlideOffsetY);
            _canvasGroup.alpha = 0f;
        }

        private void RestoreState() {
            KillActive();
            _targetRect.localScale = _originalScale;
            _targetRect.anchoredPosition = _originalAnchoredPosition;
            _canvasGroup.alpha = 1f;
        }

        private void OnDestroy() {
            KillActive();
        }

        private static CanvasGroup GetOrAddCanvasGroup(Transform t) {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg == null) cg = t.gameObject.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
