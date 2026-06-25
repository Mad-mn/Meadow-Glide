using Zenject;

namespace Feature.FirebaseModule.Scripts.Installers {
    public class FirebaseModuleInstaller : Installer<FirebaseModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<FirebaseService>()
                .AsSingle();
        }
    }
}
