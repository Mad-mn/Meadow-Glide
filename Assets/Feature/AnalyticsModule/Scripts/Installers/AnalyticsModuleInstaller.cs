using Zenject;

namespace Feature.AnalyticsModule.Scripts.Installers {
    public class AnalyticsModuleInstaller : Installer<AnalyticsModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<AnalyticsService>()
                .AsSingle();
        }
    }
}
