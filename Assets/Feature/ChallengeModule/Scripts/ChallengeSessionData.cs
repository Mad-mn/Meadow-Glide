using Feature.LevelModule.Scripts;

namespace Feature.ChallengeModule.Scripts {
    public class ChallengeSessionData {
        public bool IsActive;
        public ChallengeType ActiveChallengeType;
        public LevelConfig CurrentLevelConfig;
        public int StarsEarned;
        public bool IsCompleted;
    }
}
