using System;
using Cysharp.Threading.Tasks;
using Feature.BackgroundViewModule.Scripts;
using Feature.ByteBrewModule.Scripts;
using Feature.CameraServiceModule.Scripts;
using Feature.ChallengeModule.Scripts;
using Feature.FirebaseModule.Scripts;
using Feature.GameViewModule.Scripts;
using Feature.LevelModule.Scripts;
using Feature.LoadingViewModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.MainMenuViewModule.Scripts;
using Feature.PerfectMapViewModule.Scripts.Configs;
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
using Feature.WinLevelModule.Scripts;
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
        private readonly IChallengeConfigProvider _challengeConfigProvider;
        private readonly IResourceInfoProvider _resourceInfoProvider;
        private readonly ILocalizationService _localizationService;
        private readonly IFirebaseService _firebaseService;
        private readonly IPerfectMapRewardConfigProvider _perfectMapRewardConfigProvider;
        private readonly IByteBrewInitializeService _byteBrewInitializeService;
        private readonly IUnlockProgressConfigProvider _unlockProgressConfigProvider;
        public event Action<Type> ChangeState;

        public BootstrapState(IViewService viewService, ICameraService cameraService, ISaveDataService saveDataService,
            ISegmentStatusVisualDataProvider segmentsVisualDataProvider, ISlideAreaDataProvider slideAreaDataProvider, ILevelService levelService,
            IAudioDataProvider audioDataProvider, IAudioService audioService, IVibrationService vibrationService, IEconomyDataProvider economyDataProvider,
            IToolConfigProvider toolConfigProvider, ITransactionConfigsProvider transactionConfigsProvider, IChallengeConfigProvider challengeConfigProvider,
            IResourceInfoProvider resourceInfoProvider, ILocalizationService localizationService, IFirebaseService firebaseService,
            IPerfectMapRewardConfigProvider perfectMapRewardConfigProvider, IByteBrewInitializeService byteBrewInitializeService, IUnlockProgressConfigProvider unlockProgressConfigProvider) {
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
            _challengeConfigProvider = challengeConfigProvider;
            _resourceInfoProvider = resourceInfoProvider;
            _localizationService = localizationService;
            _firebaseService = firebaseService;
            _perfectMapRewardConfigProvider = perfectMapRewardConfigProvider;
            _byteBrewInitializeService = byteBrewInitializeService;
            _unlockProgressConfigProvider = unlockProgressConfigProvider;
        }

        public void Enter() {
            Initialize()
                .Forget();
        }

        public void Exit() { }

        private async UniTaskVoid Initialize() {
            await _firebaseService.Initialize();
            _byteBrewInitializeService.Initialize();
            _saveDataService.LoadAll();
            _localizationService.Initialize();
            await _cameraService.Initialize();
            await _viewService.Initialize();
            _viewService.ShowView<BackgroundView>(ViewType.BackgroundView);
            _viewService.ShowView<LoadingView>(ViewType.LoadingView);
            await InitializeDataProviders();
            _audioService.Initialize();
            _vibrationService.Initialize();
            await _levelService.Initialize();
            await _viewService.PrewarmView<MainMenuView>(ViewType.MainMenu);
            await _viewService.PrewarmView<GameView>(ViewType.GameView);
            ExitFromState();
        }

        private void ExitFromState() {
            var data = _levelService.GetLevelDataForCurrentLevel();
            Type nextStateType = data.LevelID == 1
                ? typeof(GameSimpleState)
                : typeof(MainMenuState);

            ChangeState?.Invoke(nextStateType);
        }

        private async UniTask InitializeDataProviders() {
            await _segmentsVisualDataProvider.Initialize();
            await _slideAreaDataProvider.Initialize();
            await _audioDataProvider.Initialize();
            await _economyDataProvider.Initialize();
            await _toolConfigProvider.Initialize();
            await _transactionConfigsProvider.Initialize();
            await _challengeConfigProvider.Initialize();
            await _resourceInfoProvider.Initialize();
            await _perfectMapRewardConfigProvider.Initialize();
            await _unlockProgressConfigProvider.Initialize();
        }
    }
}