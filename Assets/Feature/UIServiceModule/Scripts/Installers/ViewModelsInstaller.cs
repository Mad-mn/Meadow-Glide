using Feature.ConfirmBuyViewModule.Scripts;
using Feature.MessageViewModule.Scripts;
using Feature.ToolButtonViewModule.Scripts;
using Zenject;

namespace Feature.UIServiceModule.Scripts.Installers {
    public class ViewModelsInstaller : Installer<ViewModelsInstaller> {
        public override void InstallBindings() {
            Container.Bind<ConfirmBuyViewModel>()
                .AsSingle();
            Container.Bind<ToolButtonsViewModel>()
                .AsSingle();
            Container.Bind<MessageViewModel>()
                .AsSingle();
        }
    }
}