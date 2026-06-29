using Feature.MoveEfficiencyModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;

namespace Feature.PerfectMapViewModule.Scripts {
    public class PerfectMapService : IPerfectMapService {
        private readonly ISaveDataModel _saveDataModel;

        public PerfectMapService(ISaveDataModel saveDataModel) {
            _saveDataModel = saveDataModel;
        }

        public bool HasUnclaimedRewards() {
            PlayerProgressData progress = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);

            if (progress.CompletedLevels == null)
                return false;

            foreach (var kvp in progress.CompletedLevels) {
                if (kvp.Value.Status != MoveEfficiencyResult.PerfectClear)
                    continue;

                if (progress.ClaimedPerfectMapRewards != null && progress.ClaimedPerfectMapRewards.Contains(kvp.Key))
                    continue;

                return true;
            }

            return false;
        }
    }
}
