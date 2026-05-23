using Zenject;

namespace Feature.UIServiceModule.Scripts.Installers {
    public class UIModuleInstaller : Installer<UIModuleInstaller> {
        public override void InstallBindings() {
            Container.BindAddressableComponent<UIRoot>(AddressConstants.UIRoot);
            Container.BindAddressableAsset<ViewSettings>(AddressConstants.ViewSettings);
            
            Container.BindInterfacesAndSelfTo<ViewService>()
                .AsSingle();
        }
    }
}