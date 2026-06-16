using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Feature.UIServiceModule.Scripts
{
    [RequireComponent(typeof(Button))]
    public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [Header("Scale")]
        [SerializeField] private float _pressScale = 0.9f;
        [SerializeField] private float _scaleDuration = 0.1f;
        [SerializeField] private Ease _scaleEase = Ease.OutQuad;

        [Header("Punch")]
        [SerializeField] private bool _usePunch = true;
        [SerializeField] private float _punchScale = 0.1f;
        [SerializeField] private float _punchDuration = 0.2f;
        [SerializeField] private int _punchVibrato = 10;
        [SerializeField] private float _punchElasticity = 1f;

        private Button _button;
        private Vector3 _originalScale;
        private Tweener _currentTween;
        private bool _isPressed;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            _isPressed = false;
            transform.localScale = _originalScale;
        }

        private void OnDisable()
        {
            _currentTween?.Kill();
            transform.localScale = _originalScale;
            _isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable) return;

            _isPressed = true;
            _currentTween?.Kill();
            _currentTween = transform.DOScale(_originalScale * _pressScale, _scaleDuration)
                .SetEase(_scaleEase)
                .SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;

            _currentTween?.Kill();

            if (_usePunch)
            {
                transform.localScale = _originalScale;
                _currentTween = transform.DOPunchScale(
                    Vector3.one * _punchScale,
                    _punchDuration,
                    _punchVibrato,
                    _punchElasticity);
            }
            else
            {
                _currentTween = transform.DOScale(_originalScale, _scaleDuration)
                    .SetEase(_scaleEase);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPressed) return;
            _isPressed = false;

            _currentTween?.Kill();
            _currentTween = transform.DOScale(_originalScale, _scaleDuration)
                .SetEase(_scaleEase);
        }
    }
}
