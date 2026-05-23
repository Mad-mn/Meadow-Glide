using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace EditorTools
{
    public static class AddressablesNamesGenerator
    {
        private const string FilePath = "Assets/Scripts/AddressConstants.cs";

        [MenuItem("Tools/GenerateAdresablesNames")]
        public static void Generate()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;

            if (settings == null)
            {
                Debug.LogError("AddressableAssetSettings not found. Please ensure Addressables is initialized.");
                return;
            }

            List<string> addresses = new List<string>();

            foreach (var group in settings.groups)
            {
                if (group == null) continue;
                foreach (var entry in group.entries)
                {
                    if (entry == null) continue;
                    addresses.Add(entry.address);
                }
            }

            addresses = addresses.Distinct().OrderBy(a => a).ToList();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("// This file is auto-generated. Do not modify manually.");
            sb.AppendLine("public static class AddressConstants");
            sb.AppendLine("{");

            HashSet<string> usedNames = new HashSet<string>();

            foreach (var address in addresses)
            {
                string constantName = Sanitize(address);
                
                // Handle name collisions
                if (usedNames.Contains(constantName))
                {
                    int index = 1;
                    string baseName = constantName;
                    while (usedNames.Contains(constantName))
                    {
                        constantName = $"{baseName}_{index}";
                        index++;
                    }
                }
                
                usedNames.Add(constantName);
                sb.AppendLine($"    public const string {constantName} = \"{address}\";");
            }

            sb.AppendLine("}");

            string directory = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(FilePath, sb.ToString());
            AssetDatabase.Refresh();
            Debug.Log($"AddressConstants generated with {addresses.Count} addresses at {FilePath}");
        }

        private static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return "Empty";

            // Replace all non-alphanumeric with underscore
            string sanitized = Regex.Replace(input, @"[^a-zA-Z0-9]", "_");

            // Ensure it doesn't start with a number
            if (char.IsDigit(sanitized[0]))
            {
                sanitized = "_" + sanitized;
            }

            // Remove multiple consecutive underscores
            sanitized = Regex.Replace(sanitized, @"_+", "_");
            
            // Trim underscores from start and end
            sanitized = sanitized.Trim('_');

            if (string.IsNullOrEmpty(sanitized)) return "Asset";

            return sanitized;
        }
    }
}
