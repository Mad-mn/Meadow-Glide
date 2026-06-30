namespace Feature.SaveDataModule.Scripts
{
    public interface ISaveDataService
    {
        /// <summary>
        /// Pre-loads all data from disk into the SaveDataModel.
        /// Should be called during game bootstrap.
        /// </summary>
        void LoadAll();

        /// <summary>
        /// Returns true if a save file for the given type exists on disk.
        /// </summary>
        bool HasSaveData(SaveDataType type);

        /// <summary>
        /// Saves a specific data type from the Model to disk.
        /// </summary>
        void Save(SaveDataType type);

        /// <summary>
        /// Saves all data types currently in the Model to disk.
        /// </summary>
        void SaveAll();

        /// <summary>
        /// Clears data in Model and deletes the file on disk.
        /// </summary>
        void Clear(SaveDataType type);

        /// <summary>
        /// Clears all data types in Model and deletes all files on disk.
        /// </summary>
        void ClearAll();
    }
}