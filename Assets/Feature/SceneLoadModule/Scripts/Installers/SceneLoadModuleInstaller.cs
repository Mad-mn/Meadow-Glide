using Zenject;

namespace Feature.SceneLoadModule.Scripts.Installers {
    public class SceneLoadModuleInstaller : Installer<SceneLoadModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesTo<SceneLoadService>()
                .AsSingle();
        }
    }
}