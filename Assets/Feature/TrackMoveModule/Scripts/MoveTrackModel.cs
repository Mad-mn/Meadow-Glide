using System;
using Feature.LevelModule.Scripts;
using UnityEngine;

namespace Feature.TrackMoveModule.Scripts {
    public class MoveTrackModel {
        public event Action OnMovesChanged;

        public int MaxMovesForCurrentLevel { get; private set; }

        public int MovesLeft { get; private set; }

        public void CacheMovesForLevel(LevelData levelData) {
            MaxMovesForCurrentLevel = levelData.LevelConfig.MovesForLevel;
            MovesLeft = MaxMovesForCurrentLevel;
        }

        public void Move() {
            MovesLeft--;
            if (MovesLeft < 0)
                MovesLeft = 0;
            OnMovesChanged?.Invoke();
        }

        public void AddMoves(int addedMoves) {
            MovesLeft += addedMoves;
            OnMovesChanged?.Invoke();
        }
    }
}