using Zenject;

namespace Feature.UIServiceModule.Scripts.Installers {
    public class UIModuleInstaller : Installer<UIModuleInstaller> {
        public override void InstallBindings() {
            
            Container.BindInterfacesAndSelfTo<ViewService>()
                .AsSingle();
        }
    }
}