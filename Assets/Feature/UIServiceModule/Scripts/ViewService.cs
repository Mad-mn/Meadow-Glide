using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Feature.UIServiceModule.Scripts {
    public class ViewService : IViewService {
        private readonly IAddressableService _addressableService;
        private readonly UniTask<UIRoot> _uiRootTask;
        private readonly UniTask<ViewSettings> _settingsTask;
        private readonly DiContainer _container;

        private UIRoot _uiRoot;
        private ViewSettings _settings;
        private Dictionary<ViewType, ViewSettings.ViewConfigEntry> _configs;
        private readonly Dictionary<ViewType, (ViewBase view, IPresenter presenter)> _activeViews = new();

        public ViewService(
            IAddressableService addressableService, 
            UniTask<UIRoot> uiRootTask, 
            UniTask<ViewSettings> settingsTask,
            DiContainer container) {
            _addressableService = addressableService;
            _uiRootTask = uiRootTask;
            _settingsTask = settingsTask;
            _container = container;
        }

        public async UniTask Initialize() =>
            await InitializeIfNeeded();

        public void ShowView<T>(ViewType viewType) where T : ViewBase {
            ShowViewTask<T>(viewType).Forget();
        }

        private async UniTaskVoid ShowViewTask<T>(ViewType viewType) where T : ViewBase {
            await InitializeIfNeeded();

            if (_activeViews.TryGetValue(viewType, out var active)) {
                active.view.Show();
                return;
            }

            if (!_configs.TryGetValue(viewType, out var config)) {
                Debug.LogError($"No config found for {viewType}");
                return;
            }

            GameObject instance = await _addressableService.InstantiateAsync(config.Address, _uiRoot.CanvasRoot);
            
            if (instance == null) {
                Debug.LogError($"Failed to load view prefab for {viewType} at address {config.Address}");
                return;
            }

            T view = instance.GetComponent<T>();
            if (view == null) {
                Debug.LogError($"Prefab at {config.Address} does not have component {typeof(T).Name}");
                return;
            }

            _container.Inject(view);

            IPresenter presenter = CreatePresenter(config, view);
            presenter?.Initialize();

            _activeViews.Add(viewType, (view, presenter));
            view.Show();
        }

        public void HideView(ViewType viewType) {
            if (!_activeViews.TryGetValue(viewType, out var active)) return;

            active.view.Hide();

            if (active.view.DestroyOnClose) {
                active.presenter?.Dispose();
                _addressableService.ReleaseInstance(active.view.gameObject);
                _activeViews.Remove(viewType);
            }
        }

        public bool IsViewOpen(ViewType viewType) {
            return _activeViews.ContainsKey(viewType);
        }

        private async UniTask InitializeIfNeeded() {
            if (_settings != null) return;

            var (root, settings) = await UniTask.WhenAll(_uiRootTask, _settingsTask);
            
            _uiRoot = root;
            _settings = settings;
            _configs = _settings.Entries.ToDictionary(x => x.ViewType);
        }

        private IPresenter CreatePresenter(ViewSettings.ViewConfigEntry config, ViewBase view) {
            if (string.IsNullOrEmpty(config.PresenterTypeName)) {
                return null;
            }

            Type presenterType = Type.GetType(config.PresenterTypeName);
            if (presenterType == null) {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                    presenterType = assembly.GetType(config.PresenterTypeName);
                    if (presenterType != null) break;
                }
            }

            if (presenterType == null) {
                Debug.LogError($"Could not find presenter type: {config.PresenterTypeName}");
                return null;
            }

            return (IPresenter)_container.Instantiate(presenterType, new object[] { view });
        }
    }
}