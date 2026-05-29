using Zenject;

namespace Feature.CameraServiceModule.Scripts.Installers {
    public class CameraServiceModuleInstaller : Installer<CameraServiceModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<CameraService>()
                .AsSingle();
        }
    }
}