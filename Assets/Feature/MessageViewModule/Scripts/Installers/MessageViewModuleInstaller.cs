using Feature.MessageViewModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Zenject;

namespace Feature.MessageViewModule.Scripts.Installers {
    public class MessageViewModuleInstaller : Installer<MessageViewModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<MessageService>()
                .AsSingle();
        }
    }
}
