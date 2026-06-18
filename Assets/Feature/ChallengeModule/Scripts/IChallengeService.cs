using System;
using System.Collections.Generic;
using Feature.LevelModule.Scripts;
using Feature.StarModule.Scripts;
using Feature.TransactionModule.Scripts;

namespace Feature.ChallengeModule.Scripts {
    public interface IChallengeService {
        bool IsActive { get; }
        bool IsDailyChallengeAvailable();
        LevelData GetCurrentLevel();
        void ActivateDailyChallenge(LevelConfig levelConfig);
        void OnChallengeCompleted(int maxMoves, int movesUsed);
        int GetTodayStars();
        bool CanPlayToday();
        bool CanClaimReward();
        List<ResourceAmount> ClaimReward();
        TimeSpan GetTimeUntilNextDay();
        void Deactivate();
    }
}
