using Feature.ColorServiceModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Zenject;

namespace Feature.CircleModule.Scripts.Installers {
    public class CircleModuleInstaller : Installer<CircleModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<GameCircleModel>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<SlideSegmentService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<CircleRotationService>()
                .AsSingle()
                .NonLazy();

            Container.BindInterfacesAndSelfTo<CircleControllerService>()
                .AsSingle();
            
            Container.BindInterfacesAndSelfTo<CircleCompleteTrackService>()
                .AsSingle();
        }
    }
}