using Zenject;

namespace Feature.WinLevelModule.Scripts.Installers {
    public class WinLevelModuleInstaller : Installer<WinLevelModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<UnlockProgressConfigProvider>()
                .AsSingle();
        }
    }
}
