using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Feature.UIServiceModule.Editor {
    public class ViewModuleGenerator : EditorWindow {
        private string _viewName = "NewView";

        [MenuItem("Tools/UI/Create View Module")]
        public static void ShowWindow() {
            GetWindow<ViewModuleGenerator>("View Generator");
        }

        private void OnGUI() {
            GUILayout.Label("Create New MVP Module", EditorStyles.boldLabel);
            _viewName = EditorGUILayout.TextField("View Name (e.g. SettingsView)", _viewName);

            if (GUILayout.Button("Generate Module")) {
                Generate();
            }
        }

        private void Generate() {
            if (string.IsNullOrEmpty(_viewName)) {
                EditorUtility.DisplayDialog("Error", "View name cannot be empty", "OK");
                return;
            }

            string moduleFolderName = $"{_viewName}Module";
            string rootPath = Path.Combine("Assets", "Feature", moduleFolderName);
            string scriptsPath = Path.Combine(rootPath, "Scripts");
            string prefabsPath = Path.Combine(rootPath, "Prefabs");

            // Create directories
            if (!Directory.Exists(scriptsPath)) Directory.CreateDirectory(scriptsPath);
            if (!Directory.Exists(prefabsPath)) Directory.CreateDirectory(prefabsPath);

            CreateViewScript(scriptsPath);
            string presenterName = CreatePresenterScript(scriptsPath);
            UpdateViewTypeEnum();

            AssetDatabase.Refresh();

            // Note: We need to wait for compilation to actually use the new ViewType in the Inspector/SerializedProperty.
            // However, we can still try to update ViewSettings. If it fails due to missing Enum value, 
            // the user might have to run the registration part again. 
            // For now, let's try to do it.
            UpdateViewSettings(presenterName);

            EditorUtility.DisplayDialog("Success", $"Module {moduleFolderName} created and registered!", "OK");
        }

        private void CreateViewScript(string path) {
            string content = $@"using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.{_viewName}Module.Scripts {{
    public class {_viewName} : ViewBase {{
        // Add UI references here
    }}
}}";
            File.WriteAllText(Path.Combine(path, $"{_viewName}.cs"), content);
        }

        private string CreatePresenterScript(string path) {
            string presenterName = _viewName.EndsWith("View") 
                ? _viewName.Replace("View", "Presenter") 
                : $"{_viewName}Presenter";

            string content = $@"using Feature.UIServiceModule.Scripts;

namespace Feature.{_viewName}Module.Scripts {{
    public class {presenterName} : PresenterBase<{_viewName}> {{
        public {presenterName}({_viewName} view) : base(view) {{ }}

        public override void Initialize() {{
            // Initialization logic here
        }}
    }}
}}";
            File.WriteAllText(Path.Combine(path, $"{presenterName}.cs"), content);
            return presenterName;
        }

        private void UpdateViewTypeEnum() {
            string filePath = "Assets/Feature/UIServiceModule/Scripts/ViewType.cs";
            if (!File.Exists(filePath)) return;

            string content = File.ReadAllText(filePath);
            if (content.Contains(_viewName)) return;

            // Find the last numeric value
            var matches = Regex.Matches(content, @"=\s*(\d+)");
            int maxValue = 0;
            foreach (Match match in matches) {
                if (int.TryParse(match.Groups[1].Value, out int val)) {
                    if (val > maxValue) maxValue = val;
                }
            }

            int newValue = maxValue + 1;
            string newEntry = $"        {_viewName} = {newValue},\n";
            
            // Insert before the last two closing braces (enum and namespace)
            int lastBraceIndex = content.LastIndexOf('}');
            int secondLastBraceIndex = content.LastIndexOf('}', lastBraceIndex - 1);
            
            if (secondLastBraceIndex != -1) {
                content = content.Insert(secondLastBraceIndex, newEntry);
                File.WriteAllText(filePath, content);
            }
        }

        private void UpdateViewSettings(string presenterName) {
            var guids = AssetDatabase.FindAssets("t:ViewSettings");
            if (guids.Length == 0) return;

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var settings = AssetDatabase.LoadAssetAtPath<Scripts.ViewSettings>(path);
            if (settings == null) return;

            Undo.RecordObject(settings, "Add View Config");
            var serializedObject = new SerializedObject(settings);
            var entriesProp = serializedObject.FindProperty("_entries");
            
            if (entriesProp != null) {
                entriesProp.InsertArrayElementAtIndex(entriesProp.arraySize);
                var element = entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1);
                
                element.FindPropertyRelative("Address").stringValue = _viewName; 
                element.FindPropertyRelative("PresenterTypeName").stringValue = $"Feature.{_viewName}Module.Scripts.{presenterName}";
                
                // We try to set the enum value. Since we just wrote to the file, 
                // the enum might not have the new name available yet to the SerializedProperty.
                // But we can set the int value if we know the index or just leave it for the user to select.
                // Usually, insert adds a default (0). 
                
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
    }
}