using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.UIServiceModule.Scripts {
    public class ViewService : IViewService {
        private readonly IAddressableService _addressableService;
        private readonly UniTask<UIRoot> _uiRootTask;
        private readonly UniTask<ViewSettings> _settingsTask;
        private readonly DiContainer _container;
        private readonly ICameraService _cameraService;

        private UIRoot _uiRoot;
        private ViewSettings _settings;
        private Dictionary<ViewType, ViewSettings.ViewConfigEntry> _configs;
        private readonly Dictionary<ViewType, (ViewBase view, IPresenter presenter)> _activeViews = new();
        private readonly List<ViewType> _prewarmedViews = new();

        public ViewService(
            IAddressableService addressableService, 
            UniTask<UIRoot> uiRootTask, 
            UniTask<ViewSettings> settingsTask,
            DiContainer container, ICameraService cameraService) {
            _addressableService = addressableService;
            _uiRootTask = uiRootTask;
            _settingsTask = settingsTask;
            _container = container;
            _cameraService = cameraService;
        }

        public async UniTask Initialize() =>
            await InitializeIfNeeded();

        public void ShowView<T>(ViewType viewType) where T : ViewBase {
            ShowViewTask<T>(viewType).Forget();
        }

        public async UniTask<T> PrewarmView<T>(ViewType viewType) where T : ViewBase {
            await InitializeIfNeeded();
            T view = await GetOrInitializeView<T>(viewType);
            if (view != null) {
                _prewarmedViews.Add(viewType);
                view.Hide();
            }
            return view;
        }

        public void ReleasePrewarmedView(ViewType viewType) {
            if(!_prewarmedViews.Contains(viewType))
                return;
            
            if (!_activeViews.TryGetValue(viewType, out var active)) return;

            active.view.Hide();

            active.presenter?.Dispose();
            _addressableService.ReleaseInstance(active.view.gameObject);
            _activeViews.Remove(viewType);
            _prewarmedViews.Remove(viewType);
        }

        private async UniTaskVoid ShowViewTask<T>(ViewType viewType) where T : ViewBase {
            await InitializeIfNeeded();

            if (TryShow<T>(viewType))
                return;

            await GetOrInitializeView<T>(viewType);
            TryShow<T>(viewType);
        }

        private bool TryShow<T>(ViewType viewType) where T : ViewBase {
            if (_activeViews.TryGetValue(viewType, out var active)) {
                active.presenter.Show();
                active.view.Show();
                return true;
            }

            return false;
        }

        private async UniTask<T> GetOrInitializeView<T>(ViewType viewType) where T : ViewBase {
            if (_activeViews.TryGetValue(viewType, out var active)) {
                return (T)active.view;
            }

            if (!_configs.TryGetValue(viewType, out var config)) {
                Debug.LogError($"No config found for {viewType}");
                return null;
            }

            GameObject instance = await _addressableService.InstantiateAsync(config.Address, _uiRoot.CanvasRoot);
            
            if (instance == null) {
                Debug.LogError($"Failed to load view prefab for {viewType} at address {config.Address}");
                return null;
            }

            // Another path may have added this view while we were loading
            if (_activeViews.TryGetValue(viewType, out active)) {
                _addressableService.ReleaseInstance(instance);
                return (T)active.view;
            }

            T view = instance.GetComponent<T>();
            if (view == null) {
                Debug.LogError($"Prefab at {config.Address} does not have component {typeof(T).Name}");
                return null;
            }

            _container.Inject(view);

            IPresenter presenter = CreatePresenter(config, view);
            presenter?.Initialize();

            _activeViews.Add(viewType, (view, presenter));
            return view;
        }

        public void HideView(ViewType viewType) {
            if (!_activeViews.TryGetValue(viewType, out var active)) return;

            active.view.Hide();
            active.presenter.Hide();

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
            _uiRoot.SetupCamera(_cameraService.CameraObject);
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