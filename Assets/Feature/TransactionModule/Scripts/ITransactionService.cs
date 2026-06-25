
using Feature.TransactionModule.Scripts.Configs;

namespace Feature.TransactionModule.Scripts
{
    public interface ITransactionService
    {
        TransactionResult Execute(TransactionConfig config);
        TransactionResult Execute(TransactionId transactionId);
    }
}
