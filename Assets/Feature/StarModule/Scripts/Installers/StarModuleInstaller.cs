using Zenject;

namespace Feature.StarModule.Scripts.Installers {
    public class StarModuleInstaller : Installer<StarModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<MoveEfficiencyStarCalculator>()
                .AsSingle();
        }
    }
}
