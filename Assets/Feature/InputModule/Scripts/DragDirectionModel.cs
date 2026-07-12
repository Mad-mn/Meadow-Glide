using System;

namespace Feature.InputModule.Scripts {
    public enum DragDirection { None, Horizontal, Vertical }

    public class DragDirectionModel {
        public event Action<DragDirection> OnDirectionDetected;
        public event Action OnHorizontalDragFromSlide;

        public void DetectDirection(DragDirection direction) {
            OnDirectionDetected?.Invoke(direction);
        }

        public void NotifyHorizontalDragFromSlide() {
            OnHorizontalDragFromSlide?.Invoke();
        }
    }
}
