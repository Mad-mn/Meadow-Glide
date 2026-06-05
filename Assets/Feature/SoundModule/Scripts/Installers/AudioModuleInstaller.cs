using Zenject;

namespace Feature.SoundModule.Scripts.Installers {
    public class AudioModuleInstaller : Installer<AudioModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<AudioDataProvider>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<AudioService>()
                .AsSingle();
        }
    }
}