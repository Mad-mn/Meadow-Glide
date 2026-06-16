using Feature.PlayerInventoryModule.Scripts;

namespace Feature.TransactionModule.Scripts
{
    public interface IResourceStorage
    {
        int GetBalance(ResourceType type);
        bool HasEnough(ResourceType type, int amount);
        void Spend(ResourceType type, int amount);
        void Add(ResourceType type, int amount);
    }
}
