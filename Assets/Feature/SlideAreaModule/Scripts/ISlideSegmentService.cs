using Feature.CircleModule.Scripts;
using Feature.StripsModule.Scripts;

namespace Feature.SlideAreaModule.Scripts {
    public interface ISlideSegmentService {
        void RegisterCircle(CircleController circle);
        void RegisterStrip(StripController strip);
        void SetTotalStripCount(int count);
        
        void UpdateSegmentsInAreas();
        void Clear();
        bool IsSliding { get; }
    }
}