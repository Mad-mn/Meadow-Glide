using Cysharp.Threading.Tasks;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.StatusModule.Scripts.Segments;
using Feature.TutorialModule.Scripts;

namespace Feature.LevelModule.Scripts {
    public class LevelService : ILevelService {
        private readonly UniTask<LevelConfigProvider> _levelConfigProviderTask;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ITutorialService _tutorialService;
        private readonly ISegmentStatusService _segmentStatusService;
        private LevelConfigProvider _levelConfigProvider;
        
        public LevelService(UniTask<LevelConfigProvider> levelConfigProviderTask,
             ISaveDataModel saveDataModel,
             ITutorialService tutorialService,
             ISegmentStatusService segmentStatusService) {
            _levelConfigProviderTask = levelConfigProviderTask;
            _saveDataModel = saveDataModel;
            _tutorialService = tutorialService;
            _segmentStatusService = segmentStatusService;
        }

        public async UniTask Initialize() {
            _levelConfigProvider = await _levelConfigProviderTask;
        }

        public LevelData GetLevelDataForCurrentLevel() {
            PlayerProgressData playerProgressData = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            return _levelConfigProvider.LevelDatas[playerProgressData.Level];
        }

        public void LevelStarted() {
            _tutorialService.TryActivateTutorial();
            _segmentStatusService.UpdateStatus();
        }
    }
}