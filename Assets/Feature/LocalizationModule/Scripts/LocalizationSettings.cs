using Feature.LocalizationModule.Scripts.Data;
using UnityEngine;

namespace Feature.LocalizationModule.Scripts
{
    [CreateAssetMenu(fileName = "LocalizationSettings", menuName = "Configs/Localization/Settings")]
    public class LocalizationSettings : ScriptableObject
    {
        [Header("Google Sheets")]
        [Tooltip("Посилання на експорт CSV з Google Tables.\nФормат: https://docs.google.com/spreadsheets/d/{SHEET_ID}/export?format=csv&gid=0")]
        public string GoogleSheetCsvUrl;

        [Header("Paths")]
        [Tooltip("Шлях для збереження CSV файлу в проєкті")]
        public string CsvOutputPath = "Assets/Feature/LocalizationModule/Resources/Localization/Localization.csv";
    }
}