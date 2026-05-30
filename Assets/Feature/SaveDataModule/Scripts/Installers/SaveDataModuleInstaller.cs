using Zenject;

namespace Feature.SaveDataModule.Scripts.Installers
{
    public class SaveDataModuleInstaller : Installer<SaveDataModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SaveDataModel>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveDataService>().AsSingle();
        }
    }
}