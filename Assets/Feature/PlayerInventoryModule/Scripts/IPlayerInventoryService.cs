namespace Feature.PlayerInventoryModule.Scripts
{
    public interface IPlayerInventoryService
    {
        int GetBalance(ResourceType type);
        bool HasEnough(ResourceType type, int amount);
        bool TrySpend(ResourceType type, int amount);
        void Add(ResourceType type, int amount);
    }
}
