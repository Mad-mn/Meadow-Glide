using Zenject;

namespace Feature.StripsModule.Scripts.Installers {
    public class StripModuleInstaller : Installer<StripModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<StripModel>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<StripSpawnService>()
                .AsSingle();
        }
    }
}