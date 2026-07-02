using Zenject;

namespace Feature.AnalyticsModule.Scripts.Installers {
    public class AnalyticsModuleInstaller : Installer<AnalyticsModuleInstaller> {
        public override void InstallBindings() {
            Container.Bind<AnalyticsService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AnalyticsServiceDecorator>()
                .AsSingle();
        }
    }
}
