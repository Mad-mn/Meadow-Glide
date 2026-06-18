using System;
using Feature.LevelModule.Scripts;

namespace Feature.ChallengeModule.Scripts {
    public class ChallengeLevelSelector {
        private readonly ChallengeConfig _config;

        public ChallengeLevelSelector(ChallengeConfig config) {
            _config = config;
        }

        public LevelData GetLevelForDate(DateTime date) {
            if (_config.LevelPool == null || _config.LevelPool.Count == 0)
                return default;

            int dayOfYear = date.DayOfYear;
            int index = dayOfYear % _config.LevelPool.Count;
            LevelConfig levelConfig = _config.LevelPool[index];

            return new LevelData {
                LevelID = index,
                LevelConfig = levelConfig
            };
        }
    }
}
