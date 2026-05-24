using Zenject;

namespace Feature.ColorServiceModule.Scripts.Installers {
    public class CircleColorModuleInstaller : Installer<CircleColorModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<CircleColorService>()
                .AsCached();
        }
    }
}