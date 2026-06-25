using Zenject;

namespace Feature.CoroutineRunnerModule.Scripts.Installers {
    public class CoroutineRunnerInstaller : Installer<CoroutineRunnerInstaller> {
        public override void InstallBindings() {
            Container.Bind<ICoroutineRunner>()
                .To<CoroutineRunner>()
                .FromNewComponentOnNewGameObject()
                .AsSingle()
                .NonLazy();
        }
    }
}
