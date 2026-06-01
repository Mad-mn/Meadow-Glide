using Cysharp.Threading.Tasks;

namespace Feature.StatusModule.Scripts.Segments {
    public interface ISegmentStatusVisualDataProvider {
        UniTask Initialize();
        SegmentStatusVisualData GetVisualDataByStatus(SegmentStatus status);
    }
}