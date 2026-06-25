using Zenject;

namespace Feature.UndoModule.Scripts.Installers
{
    public class UndoModuleInstaller : Installer<UndoModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<UndoService>().AsSingle();
        }
    }
}
