namespace Feature.InputModule.Scripts {
    public interface IInteractionStateService {
        bool IsRotationActive { get; set; }
        bool IsSlideActive { get; set; }
        bool AnyInteractionActive => IsRotationActive || IsSlideActive;
        bool InputBlocked { get; set; }
    }

    public class InteractionStateService : IInteractionStateService {
        public bool IsRotationActive { get; set; }
        public bool IsSlideActive { get; set; }
        public bool InputBlocked { get; set; }
    }
}