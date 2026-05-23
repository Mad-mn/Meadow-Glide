using Feature.GameStateModule.Scripts.States;
using Zenject;

namespace Feature.GameStateModule.Scripts.Installers {
    public class GameStateModuleInstaller : Installer<GameStateModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<BootstrapState>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<MainMenuState>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameSimpleState>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GameStateMachine>()
                .AsSingle()
                .NonLazy();
        }
    }
}