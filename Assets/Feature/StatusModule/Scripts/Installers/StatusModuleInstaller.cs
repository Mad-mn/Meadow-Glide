using Zenject;

namespace Feature.StatusModule.Scripts.Installers {
    public class StatusModuleInstaller : Installer<StatusModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<SegmentStatusService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<SegmentStatusVisualDataProvider>()
                .AsSingle();
        }
    }
}