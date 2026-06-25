using System;
using Feature.TransactionModule.Scripts;

namespace Feature.ConfirmBuyViewModule.Scripts {
    public class ConfirmBuyViewModel {
        public event Action<TransactionId> OnSetupTransactionId;
        
        public event Action OnConfirmSuccess;
        
        public void SetupTransactionId(TransactionId transactionId) {
            OnSetupTransactionId?.Invoke(transactionId);
        }
        
        public void ConfirmSuccess() {
            OnConfirmSuccess?.Invoke();
        }
    }
}