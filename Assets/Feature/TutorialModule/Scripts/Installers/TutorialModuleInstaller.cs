using Feature.TutorialModule.Scripts.Factory;
using Feature.TutorialViewModule.Scripts;
using Zenject;

namespace Feature.TutorialModule.Scripts.Installers {
    public class TutorialModuleInstaller : Installer<TutorialModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<TutorialService>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<TutorialFactory>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<TutorialAssetProvider>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<TutorialViewModel>()
                .AsSingle();
        }
    }
}