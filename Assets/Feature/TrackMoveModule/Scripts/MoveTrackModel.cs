using System;

namespace Feature.TrackMoveModule.Scripts {
    public class MoveTrackModel {
        public event Action OnMove;

        public void Move() {
            OnMove?.Invoke();
        }
    }
}