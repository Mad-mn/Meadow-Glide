using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Feature.StatusModule.Scripts {
    public interface ISegmentStatusVisualDataProvider {
        UniTask Initialize();
        SegmentStatusVisualData GetVisualDataByStatus(SegmentStatus status);
    }
}