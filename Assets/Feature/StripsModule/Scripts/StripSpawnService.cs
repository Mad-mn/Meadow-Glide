using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.StripRotationModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.StripsModule.Scripts {
    public class StripSpawnService : IStripSpawnService {
        private readonly UniTask<StripController> _stripControllerTask;
        private readonly IInstantiator _instantiator;
        private readonly UniTask<CircleParamsConfig> _circleParamsConfigTask;
        private StripController _stripControllerPrefab;
        private CircleParamsConfig _circleParamsConfig;
        private IStripRotationService _stripRotationService;
        private ISlideSegmentService _slideSegmentService;
        private readonly StripModel _stripModel;

        public StripSpawnService(UniTask<StripController> stripControllerTask, IInstantiator instantiator, UniTask<CircleParamsConfig> circleParamsConfigTask,
            IStripRotationService stripRotationService, ISlideSegmentService slideSegmentService, StripModel stripModel) {
            _stripControllerTask = stripControllerTask;
            _instantiator = instantiator;
            _circleParamsConfigTask = circleParamsConfigTask;
            _stripRotationService = stripRotationService;
            _slideSegmentService = slideSegmentService;
            _stripModel = stripModel;
        }

        public async UniTask Initialize() {
            _circleParamsConfig = await _circleParamsConfigTask;
            _stripControllerPrefab = await _stripControllerTask;
        }

        public StripController SpawnStrip(CircleConfig config, int positionIndex) {
            StripController strip = _instantiator.InstantiatePrefabForComponent<StripController>(_stripControllerPrefab);
            float centerY = _circleParamsConfig.GetStripCenterY(positionIndex);
            float segmentHeight = _circleParamsConfig.GetUniformSegmentThickness();
            float stripLoopLength = _circleParamsConfig.StripLoopLength;

            strip.Setup(config, segmentHeight, stripLoopLength, centerY, positionIndex);

            _stripRotationService.Register(strip);
            _slideSegmentService.RegisterStrip(strip);
            _stripModel.RegisterStrip(strip);
            return strip;
        }
    }
}
