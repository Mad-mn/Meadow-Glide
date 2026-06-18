using Feature.PlayerInventoryModule.Scripts;
using Zenject;

namespace Feature.DailyChallengeStartViewModule.Scripts.Installers {
    public class DailyChallengeStartViewModuleInstaller : Installer<DailyChallengeStartViewModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<ResourceInfoProvider>()
                .AsSingle();
        }
    }
}
