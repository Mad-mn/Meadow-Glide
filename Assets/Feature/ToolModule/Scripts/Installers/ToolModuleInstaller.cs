using Feature.ToolModule.Scripts.Factory;
using Zenject;

namespace Feature.ToolModule.Scripts.Installers {
    public class ToolModuleInstaller : Installer<ToolModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<ToolService>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ToolConfigProvider>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<ToolFactory>()
                .AsSingle();
        }
    }
}