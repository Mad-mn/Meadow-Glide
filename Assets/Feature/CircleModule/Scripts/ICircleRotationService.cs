namespace Feature.CircleModule.Scripts {
    public interface ICircleRotationService {
        void Register(CircleController circle);
        void Clear();
        bool IsInteracting { get; }
    }
}