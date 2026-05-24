using Zenject;

namespace Feature.ColorServiceModule.Scripts.Installers {
    public class CircleColorModuleInstaller : Installer<CircleColorModuleInstaller> {
        public override void InstallBindings() {
            Container.BindAddressableAsset<CircleColorProvider>(AddressConstants.CircleColorProvider);
            Container.BindInterfacesAndSelfTo<CircleColorService>()
                .AsCached();
        }
    }
}