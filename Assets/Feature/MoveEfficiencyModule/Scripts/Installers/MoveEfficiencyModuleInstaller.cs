using Zenject;

namespace Feature.MoveEfficiencyModule.Scripts.Installers {
    public class MoveEfficiencyModuleInstaller : Installer<MoveEfficiencyModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<MoveEfficiencyService>()
                .AsSingle();
        }
    }
}
