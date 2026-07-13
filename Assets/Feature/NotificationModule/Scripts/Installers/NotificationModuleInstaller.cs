using Zenject;

namespace Feature.NotificationModule.Scripts.Installers {
    public class NotificationModuleInstaller : Installer<NotificationModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<AndroidNotificationScheduler>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<NotificationConfigProvider>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<NotificationService>()
                .AsSingle();
        }
    }
}
