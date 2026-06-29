using Feature.PerfectMapViewModule.Scripts.Configs;
using Zenject;

namespace Feature.PerfectMapViewModule.Scripts.Installers {
    public class PerfectMapViewModuleInstaller : Installer<PerfectMapViewModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<PerfectMapRewardConfigProvider>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<PerfectMapService>()
                .AsSingle();
            Container.Bind<PerfectMapModel>()
                .AsSingle();
        }
    }
}
