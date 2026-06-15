using Zenject;

namespace Feature.PlayerInventoryModule.Scripts.Installers {
    public class PlayerInventoryModuleInstaller : Installer<PlayerInventoryModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<PlayerInventoryModel>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<PlayerInventoryService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<EconomyDataProvider>()
                .AsSingle();
        }
    }
}