namespace Feature.SaveDataModule.Scripts
{
    public interface ISaveDataModel
    {
        T Get<T>(SaveDataType type) where T : ISaveData, new();
        ISaveData GetRaw(SaveDataType type);
        void Set(SaveDataType type, ISaveData data);
    }
}