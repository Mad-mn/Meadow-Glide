using System;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using Feature.GameViewModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.LoadingViewModule.Scripts;
using Feature.MainMenuViewModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.StateModule.Scripts.Base;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using Feature.StatusModule.Scripts.SlideAreas;
using Feature.ToolModule.Scripts;
using Feature.TransactionModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.GameStateModule.Scripts.States {
    public class BootstrapState : IState {
        private readonly IViewService _viewService;
        private readonly ICameraService _cameraService;
        private readonly ISaveDataService _saveDataService;
        private readonly ISegmentStatusVisualDataProvider _segmentsVisualDataProvider;
        private readonly ISlideAreaDataProvider _slideAreaDataProvider;
        private readonly ILevelService _levelService;
        private readonly IAudioDataProvider _audioDataProvider;
        private readonly IAudioService _audioService;
        private readonly IVibrationService _vibrationService;
        private readonly IEconomyDataProvider _economyDataProvider;
        private readonly IToolConfigProvider _toolConfigProvider;
        private readonly ITransactionConfigsProvider _transactionConfigsProvider;
        public event Action<Type> ChangeState;

        public BootstrapState(IViewService viewService, ICameraService cameraService, ISaveDataService saveDataService,
            ISegmentStatusVisualDataProvider segmentsVisualDataProvider, ISlideAreaDataProvider slideAreaDataProvider,
            ILevelService levelService, IAudioDataProvider audioDataProvider, IAudioService audioService,
            IVibrationService vibrationService, IEconomyDataProvider economyDataProvider,
            IToolConfigProvider toolConfigProvider, ITransactionConfigsProvider transactionConfigsProvider) {
            _viewService = viewService;
            _cameraService = cameraService;
            _saveDataService = saveDataService;
            _segmentsVisualDataProvider = segmentsVisualDataProvider;
            _slideAreaDataProvider = slideAreaDataProvider;
            _levelService = levelService;
            _audioDataProvider = audioDataProvider;
            _audioService = audioService;
            _vibrationService = vibrationService;
            _economyDataProvider = economyDataProvider;
            _toolConfigProvider = toolConfigProvider;
            _transactionConfigsProvider = transactionConfigsProvider;
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
            _audioService.Initialize();
            _vibrationService.Initialize();
            await _levelService.Initialize();
            await _viewService.PrewarmView<MainMenuView>(ViewType.MainMenu);
            await _viewService.PrewarmView<GameView>(ViewType.GameView);
            ChangeState?.Invoke(typeof(MainMenuState));
        }

        private async UniTask InitializeDataProviders() {
            await _segmentsVisualDataProvider.Initialize();
            await _slideAreaDataProvider.Initialize();
            await _audioDataProvider.Initialize();
            await _economyDataProvider.Initialize();
            await _toolConfigProvider.Initialize();
            await _transactionConfigsProvider.Initialize();
        }
    }
}