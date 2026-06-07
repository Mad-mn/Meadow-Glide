using Zenject;

namespace Feature.VibrationModule.Scripts.Installers {
    public class VibrationModuleInstaller : Installer<VibrationModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<VibrationService>()
                .AsSingle();
        }
    }
}