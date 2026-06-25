using Feature.ColorServiceModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StatusModule.Scripts.Segments;

namespace Feature.CircleModule.Scripts {
    public interface IGameSegment {
        float Radius { get; }
        CircleColorType ColorType { get; }
        bool IsBlocked { get; }
        float CurrentWight { get; }

        SegmentConfig GetConfig();
        void SetConfig(SegmentConfig config);
        void SetWidth(float width, bool zoomed = false);
        void SetRadius(float radius);
        void SetVisible(bool visible);
        void SetStatus(SegmentStatus status);
        SegmentStatus GetStatus();
        int GetSortingOrder();
        void SetSortingOrder(int order);
        void HideStatusIcon();
        void TriggerBlockedAnimation();
        void ZoomIn(bool force = false);
        void ZoomOut();
    }
}
