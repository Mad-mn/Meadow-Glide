using UnityEngine;

namespace Feature.InputModule.Scripts {
    public interface IDragDirectionService {
        void StartTracking(Vector2 startWorldPos);
        void Reset();
    }
}
