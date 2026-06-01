using System.Linq;
using Cysharp.Threading.Tasks;

namespace Feature.StatusModule.Scripts.SlideAreas {
    public class SlideAreaStatusDataProvider : ISlideAreaDataProvider {
        private readonly UniTask<SlideAreaStatusDataConfig> _configTask;
        private SlideAreaStatusDataConfig _config;

        public SlideAreaStatusDataProvider(UniTask<SlideAreaStatusDataConfig> configTaskTask) {
            _configTask = configTaskTask;
        }

        public async UniTask Initialize() {
            _config = await _configTask;
        }

        public SlideAreaData GetSlideAreaData(SlideAreaStatus slideAreaStatus) {
            var data = _config.SlideAreaDatas.First(data => data.SlideAreaStatus == slideAreaStatus);
            return data;
        }
    }
}