using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.LocalizationModule.Scripts.Utils;

namespace Feature.LocalizationModule.Editor
{
    public class LocalizationImporterWindow : EditorWindow
    {
        private LocalizationSettings _settings;
        private Vector2 _scrollPos;
        private string _statusMessage = "";
        private MessageType _statusType = MessageType.Info;
        private List<Language> _detectedLanguages = new List<Language>();
        private Dictionary<Language, int> _keyCounts = new Dictionary<Language, int>();

        [MenuItem("Tools/Localization/Load Localization")]
        public static void ShowWindow()
        {
            GetWindow<LocalizationImporterWindow>("Localization Importer");
        }

        private void OnEnable()
        {
            LoadOrCreateSettings();
        }

        private void LoadOrCreateSettings()
        {
            var guids = AssetDatabase.FindAssets("t:LocalizationSettings");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _settings = AssetDatabase.LoadAssetAtPath<LocalizationSettings>(path);
            }

            if (_settings == null)
            {
                _settings = ScriptableObject.CreateInstance<LocalizationSettings>();
                string folderPath = "Assets/Feature/LocalizationModule/Resources";
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                string settingsPath = $"{folderPath}/LocalizationSettings.asset";
                AssetDatabase.CreateAsset(_settings, settingsPath);
                AssetDatabase.SaveAssets();
                Debug.Log("[Localization] Created LocalizationSettings asset");
            }
        }

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("Google Sheets Localization Importer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (_settings == null)
            {
                EditorGUILayout.HelpBox("Settings asset not found. Click 'Reload Settings'.", MessageType.Warning);
                if (GUILayout.Button("Reload Settings"))
                {
                    LoadOrCreateSettings();
                }
                EditorGUILayout.EndScrollView();
                return;
            }

            EditorGUILayout.LabelField("Google Sheets CSV URL:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Формат посилання:\n" +
                "https://docs.google.com/spreadsheets/d/{SHEET_ID}/export?format=csv&gid=0\n\n" +
                "Де {SHEET_ID} — це ID вашої таблиці з URL.",
                MessageType.Info);

            EditorGUI.BeginChangeCheck();
            _settings.GoogleSheetCsvUrl = EditorGUILayout.TextField("URL:", _settings.GoogleSheetCsvUrl);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_settings);
            }

            if (GUILayout.Button("Як отримати посилання?"))
            {
                Application.OpenURL("https://support.google.com/docs/answer/183965");
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Output CSV Path:", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _settings.CsvOutputPath = EditorGUILayout.TextField("Path:", _settings.CsvOutputPath);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_settings);
            }

            GUILayout.Space(15);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Download and Import", GUILayout.Height(35)))
            {
                DownloadAndImport();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import from Local CSV"))
            {
                ImportFromLocalCsv();
            }
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (_detectedLanguages.Count > 0)
            {
                EditorGUILayout.LabelField("Detected Languages:", EditorStyles.boldLabel);
                foreach (var lang in _detectedLanguages)
                {
                    int count = _keyCounts.ContainsKey(lang) ? _keyCounts[lang] : 0;
                    EditorGUILayout.LabelField($"  {lang}: {count} keys");
                }
            }

            GUILayout.Space(10);

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, _statusType);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Save Settings"))
            {
                EditorUtility.SetDirty(_settings);
                AssetDatabase.SaveAssets();
                ShowStatus("Settings saved!", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DownloadAndImport()
        {
            if (string.IsNullOrEmpty(_settings.GoogleSheetCsvUrl))
            {
                ShowStatus("Будь ласка, введіть посилання на Google Tables", MessageType.Error);
                return;
            }

            try
            {
                ShowStatus("Завантаження з Google Sheets...", MessageType.Info);

                using (var client = new WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    string csvContent = client.DownloadString(_settings.GoogleSheetCsvUrl);
                    ProcessCsvContent(csvContent);
                    SaveCsvToFile(csvContent);
                }

                LocalizationValidator.SyncEnumFromCsv();
                ShowStatus("Успішно імпортовано з Google Sheets!", MessageType.Info);
            }
            catch (System.Exception e)
            {
                ShowStatus($"Помилка завантаження: {e.Message}", MessageType.Error);
                Debug.LogError($"[Localization] Download failed: {e}");
            }
        }

        private void ImportFromLocalCsv()
        {
            string path = EditorUtility.OpenFilePanel("Оберіть CSV файл", "", "csv");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string csvContent = File.ReadAllText(path, Encoding.UTF8);
                ProcessCsvContent(csvContent);

                // Копіюємо файл в проєкт
                string projectPath = Path.Combine(Application.dataPath, "..", _settings.CsvOutputPath);
                string directory = Path.GetDirectoryName(projectPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                File.Copy(path, projectPath, true);
                AssetDatabase.Refresh();

                LocalizationValidator.SyncEnumFromCsv();
                ShowStatus("Успішно імпортовано з локального CSV!", MessageType.Info);
            }
            catch (System.Exception e)
            {
                ShowStatus($"Помилка читання файлу: {e.Message}", MessageType.Error);
                Debug.LogError($"[Localization] Import failed: {e}");
            }
        }

        private void ProcessCsvContent(string csvContent)
        {
            var (languages, data) = CsvParser.Parse(csvContent);

            _detectedLanguages = languages;
            _keyCounts.Clear();

            foreach (var kvp in data)
            {
                _keyCounts[kvp.Key] = kvp.Value.Count;
            }

            Debug.Log($"[Localization] Parsed {languages.Count} languages, {_keyCounts.Values.Count} key groups");
        }

        private void SaveCsvToFile(string csvContent)
        {
            string fullPath = Path.Combine(Application.dataPath, "..", _settings.CsvOutputPath);
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, csvContent, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"[Localization] CSV saved to: {_settings.CsvOutputPath}");
        }

        private void ShowStatus(string message, MessageType type)
        {
            _statusMessage = message;
            _statusType = type;
            Repaint();
        }
    }
}