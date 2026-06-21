using System;
using System.Collections.Generic;
using Feature.LevelModule.Scripts;
using Feature.MoveEfficiencyModule.Scripts;
using Feature.TransactionModule.Scripts;

namespace Feature.ChallengeModule.Scripts {
    public interface IChallengeService {
        bool IsActive { get; }
        bool IsDailyChallengeAvailable();
        LevelData GetCurrentLevel();
        int GetMinMoves();
        void ActivateDailyChallenge(LevelConfig levelConfig);
        void OnChallengeCompleted(MoveEfficiencyResult result);
        MoveEfficiencyResult GetTodayBestResult();
        MoveEfficiencyResult GetPreviousClaimedResult();
        bool CanPlayToday();
        bool CanClaimReward();
        List<ResourceAmount> ClaimReward();
        TimeSpan GetTimeUntilNextDay();
        void Deactivate();
    }
}
