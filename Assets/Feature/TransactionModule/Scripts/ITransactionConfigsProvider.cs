using Cysharp.Threading.Tasks;
using Feature.TransactionModule.Scripts.Configs;

namespace Feature.TransactionModule.Scripts
{
    public interface ITransactionConfigsProvider
    {
        UniTask Initialize();
        TransactionConfig GetConfig(TransactionId transactionId);
    }
}
