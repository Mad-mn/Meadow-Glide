using Zenject;

namespace Feature.TransactionModule.Scripts.Installers
{
    public class TransactionModuleInstaller : Installer<TransactionModuleInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<TransactionConfigsProvider>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<TransactionService>()
                .AsSingle();
            
            Container.BindInterfacesAndSelfTo<PriceDataProvider>()
                .AsSingle();
        }
    }
}
