namespace Feature.InputModule.Scripts {
    public interface IInteractionStateService {
        bool IsRotationActive { get; set; }
        bool IsSlideActive { get; set; }
        bool AnyInteractionActive => IsRotationActive || IsSlideActive;
        bool InputBlocked { get; }
        int AllowedStripIndex { get; set; }
        int AllowedSlideAreaIndex { get; set; }
        void BlockInput();
        void UnblockInput();
        void ResetInputBlock();
        void ResetFilters();
    }

    public class InteractionStateService : IInteractionStateService {
        private int _blockCount;

        public bool IsRotationActive { get; set; }
        public bool IsSlideActive { get; set; }
        public bool InputBlocked => _blockCount > 0;
        public int AllowedStripIndex { get; set; } = -1;
        public int AllowedSlideAreaIndex { get; set; } = -1;

        public void BlockInput() {
            _blockCount++;
        }

        public void UnblockInput() {
            _blockCount--;
            if (_blockCount < 0) _blockCount = 0;
        }

        public void ResetInputBlock() {
            _blockCount = 0;
        }

        public void ResetFilters() {
            AllowedStripIndex = -1;
            AllowedSlideAreaIndex = -1;
        }
    }
}
