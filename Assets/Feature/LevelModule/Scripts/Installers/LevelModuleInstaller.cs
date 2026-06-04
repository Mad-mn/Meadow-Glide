using Zenject;

namespace Feature.LevelModule.Scripts.Installers {
    public class LevelModuleInstaller : Installer<LevelModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<LevelService>()
                .AsSingle();

            Container.Bind<LevelModel>()
                .AsSingle();
        }
    }
}