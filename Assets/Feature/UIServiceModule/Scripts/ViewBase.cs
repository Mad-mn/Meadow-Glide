using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Feature.UIServiceModule.Scripts {
    public interface IView {
        UniTask ShowAsync();
        void Hide();
        UniTask HideAsync();
        bool DestroyOnClose { get; }
        bool UseAnimation { get; }
    }

    public abstract class ViewBase : MonoBehaviour, IView {
        [SerializeField] private bool _destroyOnClose;
        [SerializeField] private bool _useAnimation;

        public bool DestroyOnClose => _destroyOnClose;
        public bool UseAnimation => _useAnimation;

        private ViewTransition _transition;

        protected virtual void Awake() {
            if (_useAnimation) {
                _transition = GetComponent<ViewTransition>();
                if (_transition == null) {
                    _transition = gameObject.AddComponent<ViewTransition>();
                }
            }
        }

        public virtual async UniTask ShowAsync() {
            gameObject.SetActive(true);
            if (_useAnimation && _transition != null) {
                await _transition.PlayShow();
            }
        }

        public virtual void Hide() {
            gameObject.SetActive(false);
        }

        public virtual async UniTask HideAsync() {
            if (_useAnimation && _transition != null) {
                await _transition.PlayHide();
            }
            gameObject.SetActive(false);
        }
    }
}
