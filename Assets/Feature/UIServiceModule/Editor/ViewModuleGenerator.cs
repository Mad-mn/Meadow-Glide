using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

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

            AssetDatabase.Refresh();

            // Note: We might need to wait for compilation to get the types, 
            // but we can at least add the entry with strings/default values.
            UpdateViewSettings(presenterName);

            EditorUtility.DisplayDialog("Success", $"Module {moduleFolderName} created!", "OK");
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

        private void UpdateViewSettings(string presenterName) {
            var guids = AssetDatabase.FindAssets("t:ViewSettings");
            if (guids.Length == 0) {
                Debug.LogWarning("ViewSettings asset not found. Please create one manually.");
                return;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var settings = AssetDatabase.LoadAssetAtPath<Scripts.ViewSettings>(path);

            if (settings == null) return;

            // Since we can't easily add to Enum via script without re-compilation,
            // we just add a new entry to the list if possible.
            // Note: You might need to manually update ViewType enum first.
            
            Undo.RecordObject(settings, "Add View Config");
            
            // We use reflection or a helper to add to the private list if needed, 
            // but let's assume we can add to a public list or handle serialized property.
            var serializedObject = new SerializedObject(settings);
            var entriesProp = serializedObject.FindProperty("_entries");
            
            if (entriesProp != null) {
                entriesProp.InsertArrayElementAtIndex(entriesProp.arraySize);
                var element = entriesProp.GetArrayElementAtIndex(entriesProp.arraySize - 1);
                
                // Set default values. ViewType will be 0 (first value).
                element.FindPropertyRelative("Address").stringValue = _viewName; 
                element.FindPropertyRelative("PresenterTypeName").stringValue = $"Feature.{_viewName}Module.Scripts.{presenterName}";
                
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
            }
        }
    }
}