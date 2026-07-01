using System;
using Cysharp.Threading.Tasks;

namespace Feature.UIServiceModule.Scripts {
    public abstract class PresenterBase<TView> : IPresenter, IDisposable where TView : IView {
        protected TView View { get; private set; }

        protected PresenterBase(TView view) {
            View = view;
        }

        public abstract void Initialize();

        public virtual void Dispose() {
            View = default;
        }

        public virtual void Show() { }
        public virtual void Hide() { }
    }
}
