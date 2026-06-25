using Feature.PreGamePlacementModule.Scripts;
using Zenject;

namespace Feature.PreGamePlacementModule.Scripts.Installers {
    public class PreGamePlacementModuleInstaller : Installer<PreGamePlacementModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<PreGamePlacementService>().AsSingle();
        }
    }
}
