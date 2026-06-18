using Zenject;

namespace Feature.LevelResultModule.Scripts.Installers {
    public class LevelResultModuleInstaller : Installer<LevelResultModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<LevelResultService>()
                .AsSingle();
        }
    }
}
