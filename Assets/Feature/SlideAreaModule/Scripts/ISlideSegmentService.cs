using Feature.CircleModule.Scripts;

namespace Feature.SlideAreaModule.Scripts {
    public interface ISlideSegmentService {
        void RegisterCircle(CircleController circle);
        
        void UpdateSegmentsInAreas();
        void Clear();
        bool IsSliding { get; }
    }
}