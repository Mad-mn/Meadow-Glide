using Feature.LevelModule.Scripts;
using Feature.MoveEfficiencyModule.Scripts;

namespace Feature.ChallengeModule.Scripts {
    public class ChallengeSessionData {
        public bool IsActive;
        public ChallengeType ActiveChallengeType;
        public LevelConfig CurrentLevelConfig;
        public MoveEfficiencyResult CurrentResult;
        public MoveEfficiencyResult PreviousClaimedResult;
        public bool IsCompleted;
    }
}
