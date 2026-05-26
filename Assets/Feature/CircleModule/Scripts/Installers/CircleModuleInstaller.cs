using Feature.SlideAreaModule.Scripts;
using Zenject;

namespace Feature.CircleModule.Scripts.Installers {
    public class CircleModuleInstaller : Installer<CircleModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<SlideSegmentService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<CircleRotationService>().AsSingle().NonLazy();
        }
    }
}