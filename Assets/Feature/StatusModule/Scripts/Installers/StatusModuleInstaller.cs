using Feature.StatusModule.Scripts.Segments;
using Feature.StatusModule.Scripts.SlideAreas;
using Zenject;

namespace Feature.StatusModule.Scripts.Installers {
    public class StatusModuleInstaller : Installer<StatusModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<SegmentStatusVisualDataProvider>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<SlideAreaStatusDataProvider>()
                .AsSingle();
            
        }
    }
}