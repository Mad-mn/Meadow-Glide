using Zenject;

namespace Feature.SlideAreaModule.Scripts.Installers {
    public class SlideAreaModuleInstaller : Installer<SlideAreaModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<SlideAreaService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<SlideAreaModel>()
                .AsSingle();
        }
    }
}