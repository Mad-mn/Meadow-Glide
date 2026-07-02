using Zenject;

namespace Feature.ByteBrewModule.Scripts.Installers {
    public class ByteBrewModuleInstaller : Installer<ByteBrewModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<ByteBrewInitializeService>()
                .AsSingle();
        }
    }
}