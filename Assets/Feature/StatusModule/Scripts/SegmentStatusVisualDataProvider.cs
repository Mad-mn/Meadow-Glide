using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Feature.StatusModule.Scripts {
    public class SegmentStatusVisualDataProvider : ISegmentStatusVisualDataProvider {
        private readonly UniTask<SegmentStatusVisualConfig> _segmentStatusVisualDataProviderTask;
        private SegmentStatusVisualConfig _config;

        public SegmentStatusVisualDataProvider(UniTask<SegmentStatusVisualConfig> segmentStatusVisualDataProviderTask) {
            _segmentStatusVisualDataProviderTask = segmentStatusVisualDataProviderTask;
        }

        public async UniTask Initialize() {
            _config = await _segmentStatusVisualDataProviderTask;
        }

        public SegmentStatusVisualData GetVisualDataByStatus(SegmentStatus status) {
            SegmentStatusVisualData data = _config.SegmentStatusVisualDatas.FirstOrDefault(x => x.SegmentStatus == status);
            return data;
        }
    }
}