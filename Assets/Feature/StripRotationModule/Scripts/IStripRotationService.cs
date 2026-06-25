using Feature.StripsModule.Scripts;

namespace Feature.StripRotationModule.Scripts {
    public interface IStripRotationService {
        void Register(StripController strip);
        void Clear();
        bool IsInteracting { get; }
    }
}