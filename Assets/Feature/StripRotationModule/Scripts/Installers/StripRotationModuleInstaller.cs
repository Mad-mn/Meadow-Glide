using Zenject;

namespace Feature.StripRotationModule.Scripts.Installers {
    public class StripRotationModuleInstaller : Installer<StripRotationModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<StripRotationService>()
                .AsSingle();
        }
    }
}