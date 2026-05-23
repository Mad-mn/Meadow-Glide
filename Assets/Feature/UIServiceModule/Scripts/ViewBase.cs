using UnityEngine;

namespace Feature.UIServiceModule.Scripts {
    public interface IView {
        void Show();
        void Hide();
        bool DestroyOnClose { get; }
    }

    public abstract class ViewBase : MonoBehaviour, IView {
        [SerializeField] private bool _destroyOnClose;
        public bool DestroyOnClose => _destroyOnClose;

        public virtual void Show() => gameObject.SetActive(true);
        public virtual void Hide() => gameObject.SetActive(false);
    }
}