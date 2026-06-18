using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Feature.LocalizationModule.Scripts.Data;
using Feature.LocalizationModule.Scripts.Utils;

namespace Feature.LocalizationModule.Editor
{
    public static class LocalizationValidator
    {
        private const string CSV_PATH = "Assets/Feature/LocalizationModule/Resources/Localization/Localization.csv";

        [MenuItem("Tools/Localization/Validate Localization")]
        public static void Validate()
        {
            if (!File.Exists(CSV_PATH))
            {
                Debug.LogError("[Localization] CSV file not found. Run 'Load Localization' first.");
                return;
            }

            string csvContent = File.ReadAllText(CSV_PATH, Encoding.UTF8);
            var (languages, data) = CsvParser.Parse(csvContent);

            var warnings = new List<string>();
            var errors = new List<string>();

            var allEnumValues = System.Enum.GetValues(typeof(LocalizationKey));
            var csvKeys = new HashSet<LocalizationKey>();

            foreach (var langData in data.Values)
            {
                foreach (var key in langData.Keys)
                {
                    csvKeys.Add(key);
                }
            }

            foreach (LocalizationKey enumKey in allEnumValues)
            {
                if (enumKey == LocalizationKey.None) continue;

                if (!csvKeys.Contains(enumKey))
                {
                    errors.Add($"Key '{enumKey}' exists in enum but not in CSV");
                }
            }

            foreach (var key in csvKeys)
            {
                bool found = false;
                foreach (LocalizationKey enumKey in allEnumValues)
                {
                    if (enumKey == key)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    warnings.Add($"Key '{key}' exists in CSV but not in enum");
                }
            }

            foreach (var lang in languages)
            {
                if (!data.ContainsKey(lang)) continue;
                foreach (var key in data[lang].Keys)
                {
                    if (string.IsNullOrEmpty(data[lang][key]))
                    {
                        warnings.Add($"Empty translation for {lang}/{key}");
                    }
                }
            }

            if (errors.Count > 0)
            {
                Debug.LogError($"[Localization] Validation failed with {errors.Count} errors:");
                foreach (var error in errors)
                {
                    Debug.LogError($"  - {error}");
                }
            }

            if (warnings.Count > 0)
            {
                Debug.LogWarning($"[Localization] Validation warnings ({warnings.Count}):");
                foreach (var warning in warnings)
                {
                    Debug.LogWarning($"  - {warning}");
                }
            }

            if (errors.Count == 0 && warnings.Count == 0)
            {
                Debug.Log("[Localization] Validation passed! All keys are synchronized.");
            }

            EditorUtility.DisplayDialog("Validation Complete",
                $"Errors: {errors.Count}\nWarnings: {warnings.Count}\n\nCheck Console for details.",
                "OK");
        }

        public static void SyncEnumFromCsv()
        {
            if (!File.Exists(CSV_PATH))
            {
                Debug.LogError("[Localization] CSV file not found. Run 'Load Localization' first.");
                return;
            }

            string csvContent = File.ReadAllText(CSV_PATH, Encoding.UTF8);
            var lines = csvContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length < 2)
            {
                Debug.LogError("[Localization] CSV is empty or has no data rows.");
                return;
            }

            var csvEntries = new List<(int id, string keyName)>();
            for (int i = 1; i < lines.Length; i++)
            {
                var columns = ParseCsvLine(lines[i]);
                if (columns.Length < 2) continue;

                if (int.TryParse(columns[0], out int id) && !string.IsNullOrEmpty(columns[1]))
                {
                    csvEntries.Add((id, columns[1]));
                }
            }

            string enumPath = "Assets/Feature/LocalizationModule/Scripts/Data/LocalizationKey.cs";
            if (!File.Exists(enumPath))
            {
                Debug.LogError($"[Localization] Enum file not found at {enumPath}");
                return;
            }

            string enumContent = File.ReadAllText(enumPath);
            var existingEntries = new Dictionary<int, string>();

            var enumLines = enumContent.Split('\n');
            foreach (var line in enumLines)
            {
                string trimmed = line.Trim();
                if (trimmed.Contains("=") && trimmed.Contains(","))
                {
                    var parts = trimmed.Split('=');
                    string keyName = parts[0].Trim();
                    string valueStr = parts[1].Split(',')[0].Trim();

                    if (!string.IsNullOrEmpty(keyName) && int.TryParse(valueStr, out int val))
                    {
                        existingEntries[val] = keyName;
                    }
                }
            }

            var newEntries = new List<(int id, string keyName)>();
            foreach (var entry in csvEntries)
            {
                if (!existingEntries.ContainsKey(entry.id))
                {
                    newEntries.Add(entry);
                }
            }

            if (newEntries.Count == 0)
            {
                Debug.Log("[Localization] Enum is already up to date with CSV.");
                EditorUtility.DisplayDialog("Sync Complete", "Enum is already up to date.", "OK");
                return;
            }

            var newEnumEntries = new List<string>();
            foreach (var entry in newEntries)
            {
                newEnumEntries.Add($"        {entry.keyName} = {entry.id},");
            }

            int lastBraceIndex = enumContent.LastIndexOf('}');
            int secondLastBraceIndex = enumContent.LastIndexOf('}', lastBraceIndex - 1);

            string insertText = string.Join("\n", newEnumEntries) + "\n";
            enumContent = enumContent.Insert(secondLastBraceIndex, insertText);

            File.WriteAllText(enumPath, enumContent);
            AssetDatabase.Refresh();

            Debug.Log($"[Localization] Added {newEntries.Count} new keys to enum: {string.Join(", ", newEntries.Select(e => e.keyName))}");
            EditorUtility.DisplayDialog("Sync Complete", $"Added {newEntries.Count} new keys to enum.", "OK");
        }

        private static string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"' && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else if (c == '"')
                    {
                        inQuotes = false;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else if (c == ',')
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}