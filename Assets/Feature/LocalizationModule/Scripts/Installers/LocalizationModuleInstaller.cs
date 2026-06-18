using Feature.SaveDataModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.LocalizationModule.Scripts.Installers
{
    public class LocalizationModuleInstaller : Installer<LocalizationModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<LocalizationDatabase>()
                .AsSingle();
            Container.BindInterfacesAndSelfTo<LocalizationService>()
                .AsSingle();
        }
    }
}