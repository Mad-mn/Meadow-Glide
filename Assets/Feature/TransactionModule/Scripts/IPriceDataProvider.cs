namespace Feature.TransactionModule.Scripts {
    public interface IPriceDataProvider {
        int GetPrice(TransactionId transactionId);
    }
}