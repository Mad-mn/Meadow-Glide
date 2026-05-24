using Zenject;

namespace Feature.CircleModule.Scripts.Installers {
    public class CircleModuleInstaller : Installer<CircleModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<CircleRotationService>().AsSingle().NonLazy();
        }
    }
}