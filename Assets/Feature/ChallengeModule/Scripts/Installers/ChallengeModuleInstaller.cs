using Zenject;

namespace Feature.ChallengeModule.Scripts.Installers {
    public class ChallengeModuleInstaller : Installer<ChallengeModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<ChallengeService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ChallengeConfigProvider>()
                .AsSingle();
        }
    }
}
