namespace Feature.TransactionModule.Scripts
{
    public enum TransactionFailureReason
    {
        None = 0,
        NotEnoughCoins,
        NotEnoughResource,
        ProductUnavailable,
        RequirementNotMet,
        InventoryFull,
        UnknownError
    }
}
