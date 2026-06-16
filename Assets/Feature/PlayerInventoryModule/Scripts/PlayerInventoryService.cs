using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TransactionModule.Scripts;

namespace Feature.PlayerInventoryModule.Scripts
{
    public class PlayerInventoryService : IPlayerInventoryService
    {
        private readonly PlayerInventoryModel _model;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;

        public PlayerInventoryService(
            PlayerInventoryModel model,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService)
        {
            _model = model;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
        }

        private void EnsureLoaded()
        {
            if (_model.IsLoaded) return;
            var data = _saveDataModel.Get<PlayerInventoryData>(SaveDataType.PlayerInventory);
            _model.LoadFrom(data.Balances);
        }

        public int GetBalance(ResourceType type)
        {
            EnsureLoaded();
            return _model.GetBalance(type);
        }

        public bool HasEnough(ResourceType type, int amount)
        {
            EnsureLoaded();
            return _model.GetBalance(type) >= amount;
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            EnsureLoaded();
            if (_model.GetBalance(type) < amount) return false;
            int current = _model.GetBalance(type);
            _model.SetBalance(type, current - amount);
            Persist();
            return true;
        }

        public void Add(ResourceType type, int amount)
        {
            EnsureLoaded();
            if(!ConfirmForAdd(type))
                return;
            
            int current = _model.GetBalance(type);
            _model.SetBalance(type, current + amount);
            Persist();
        }

        private bool ConfirmForAdd(ResourceType type) {
            return type is not (ResourceType.ExtraMoves or ResourceType.None);
        }

        private void Persist()
        {
            PlayerInventoryData data = _saveDataModel.Get<PlayerInventoryData>(SaveDataType.PlayerInventory);
            data.Balances = _model.GetAll();
            _saveDataService.Save(SaveDataType.PlayerInventory);
        }
    }
}
