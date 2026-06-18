namespace Feature.LocalizationModule.Scripts.Data
{
    [System.Serializable]
    public struct LocalizationEntry
    {
        public LocalizationKey Key;
        public string Value;

        public LocalizationEntry(LocalizationKey key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}