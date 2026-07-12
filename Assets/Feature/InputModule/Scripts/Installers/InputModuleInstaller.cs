using Zenject;

namespace Feature.InputModule.Scripts.Installers {
    public class InputModuleInstaller : Installer<InputModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<InteractionStateService>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<DragDirectionModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<DragDirectionService>().AsSingle().NonLazy();
        }
    }
}