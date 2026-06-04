using System;

namespace Feature.LevelModule.Scripts {
    public class LevelModel {
        public event Action OnLevelStart;
        public event Action OnLevelEnd;

        public void StartLevel() {
            OnLevelStart?.Invoke();
        }

        public void EndLevel() {
            OnLevelEnd?.Invoke();
        }
    }
}