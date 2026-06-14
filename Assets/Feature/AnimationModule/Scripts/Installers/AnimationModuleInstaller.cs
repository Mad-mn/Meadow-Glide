using Zenject;

namespace Feature.AnimationModule.Scripts.Installers {
    public class AnimationModuleInstaller : Installer<AnimationModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<AnimationService>().AsSingle();
        }
    }
}
