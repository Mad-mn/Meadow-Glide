using System;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using Feature.GameViewModule.Scripts;
using Feature.LoadingViewModule.Scripts;
using Feature.MainMenuViewModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.StateModule.Scripts.Base;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using Feature.StatusModule.Scripts.SlideAreas;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.GameStateModule.Scripts.States {
    public class BootstrapState : IState {
        private readonly IViewService _viewService;
        private readonly ICameraService _cameraService;
        private readonly ISaveDataService _saveDataService;
        private readonly ISegmentStatusVisualDataProvider _segmentsVisualDataProvider;
        private readonly ISlideAreaDataProvider _slideAreaDataProvider;
        public event Action<Type> ChangeState;

        public BootstrapState(IViewService viewService, ICameraService cameraService, ISaveDataService saveDataService,
            ISegmentStatusVisualDataProvider segmentsVisualDataProvider, ISlideAreaDataProvider slideAreaDataProvider) {
            _viewService = viewService;
            _cameraService = cameraService;
            _saveDataService = saveDataService;
            _segmentsVisualDataProvider = segmentsVisualDataProvider;
            _slideAreaDataProvider = slideAreaDataProvider;
        }

        public void Enter() {
            Initialize()
                .Forget();
        }

        public void Exit() { }

        private async UniTaskVoid Initialize() {
            _saveDataService.LoadAll();
            await _cameraService.Initialize();
            await _viewService.Initialize();

            _viewService.ShowView<LoadingView>(ViewType.LoadingView);
            await InitializeDataProviders();
            await _viewService.PrewarmView<MainMenuView>(ViewType.MainMenu);
            await _viewService.PrewarmView<GameView>(ViewType.GameView);
            ChangeState?.Invoke(typeof(MainMenuState));
        }

        private async UniTask InitializeDataProviders() {
            await _segmentsVisualDataProvider.Initialize();
            await _slideAreaDataProvider.Initialize();
        }
    }
}