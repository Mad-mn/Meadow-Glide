using Zenject;

namespace Feature.AddressableModule.Scripts.Installers {
    public class AddressableModuleInstaller : Installer<AddressableModuleInstaller>{
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<AddressableService>()
                .AsSingle();
        }
    }
}