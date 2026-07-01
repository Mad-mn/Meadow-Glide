namespace Feature.LevelResultModule.Scripts {
    public interface ILevelResultService {
        void OnLevelWon();
        void OnLevelLost();
        void Reset();
        void ResetLose();
    }
}
