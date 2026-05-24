using Zenject;

namespace Feature.InputModule.Scripts.Installers {
    public class InputModuleInstaller : Installer<InputModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<InputService>().AsSingle().NonLazy();
        }
    }
}