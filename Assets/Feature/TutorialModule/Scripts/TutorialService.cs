using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.LevelModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TutorialModule.Scripts.Factory;
using Feature.TutorialModule.Scripts.Tutorials;
using Feature.TutorialViewModule.Scripts;

namespace Feature.TutorialModule.Scripts {
    public class TutorialService : ITutorialService {
        private readonly ISaveDataModel _saveDataModel;
        private readonly ITutorialFactory _factory;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly TutorialViewModel _viewModel;
        private LevelData _currentLevelData;

        public TutorialService(ISaveDataModel saveDataModel, ITutorialFactory factory,
            ITutorialAssetProvider tutorialAssetProvider, TutorialViewModel viewModel) {
            _saveDataModel = saveDataModel;
            _factory = factory;
            _tutorialAssetProvider = tutorialAssetProvider;
            _viewModel = viewModel;
        }

        public async UniTask Initialize(LevelData currentLevelData) {
            _currentLevelData = currentLevelData; 

            if(GetTutorialType() == TutorialType.None)
                return;
            await _tutorialAssetProvider.Initialize();
            await _tutorialAssetProvider.PrewarmAssets(GetAssetsForPrewarm());
        }

        public void TryActivateTutorial() {
            if(IsTutorialCompleted)
                return;

            TutorialType tutorialType = GetTutorialType();
            if(tutorialType == TutorialType.None)
                return;
            
            ActivateTutorial(tutorialType);
        }

        private void ActivateTutorial(TutorialType tutorialType) {
           _viewModel.SetTextZones(_currentLevelData.LevelConfig.TutorialLevelConfig?.TextZones);
           ITutorial tutorial = _factory.CreateTutorial(tutorialType);
           if(tutorial is null)
               return;

           tutorial.OnComplete += HandleCompleteTutorial;
           tutorial.Activate();
        }

        public void Deinitialize() {
            _tutorialAssetProvider.ReleaseAssets();
        }

        private IReadOnlyList<TutorialAssetType> GetAssetsForPrewarm() {
            return _currentLevelData.LevelConfig.TutorialLevelConfig.AssetsForTutorial;
        }

        private void HandleCompleteTutorial() {
            
        }

        private TutorialType GetTutorialType() {
            if(_currentLevelData.LevelConfig.TutorialLevelConfig is null)
                return TutorialType.None;
            
            return _currentLevelData.LevelConfig.TutorialLevelConfig.TutorialType;
        }

        private bool IsTutorialCompleted {
            get {
                return false;
            }
        }
    }
}