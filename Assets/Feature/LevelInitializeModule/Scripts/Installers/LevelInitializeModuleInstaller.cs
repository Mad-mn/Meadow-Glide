using Zenject;

namespace Feature.LevelInitializeModule.Scripts.Installers {
    public class LevelInitializeModuleInstaller : Installer<LevelInitializeModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<LevelInitializeService>().AsSingle();
        }
    }
}