using System;
using Feature.LevelModule.Scripts;
using UnityEngine;

namespace Feature.TrackMoveModule.Scripts {
    public class MoveTrackModel {
        public event Action OnMove;

        public int MaxMovesForCurrentLevel { get; private set; }

        public int MovesLeft { get; private set; }

        public void CacheMovesForLevel(LevelData levelData) {
            MaxMovesForCurrentLevel = levelData.LevelConfig.Difficulty;
            MovesLeft = MaxMovesForCurrentLevel;
        }

        public void Move() {
            MovesLeft--;
            OnMove?.Invoke();
        }
    }
}