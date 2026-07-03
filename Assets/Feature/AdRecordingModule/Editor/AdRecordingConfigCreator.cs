using Feature.AdRecordingModule.Scripts;
using UnityEditor;
using UnityEngine;

namespace Feature.AdRecordingModule.Editor {
    public static class AdRecordingConfigCreator {
        private const string ResourceFolder = "Assets/Resources";
        private const string AssetPath = ResourceFolder + "/AdRecordingConfig.asset";

        [MenuItem("Tools/Ad Recording/Create Config")]
        public static void CreateConfig() {
            if (!AssetDatabase.IsValidFolder(ResourceFolder)) {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            var existing = AssetDatabase.LoadAssetAtPath<AdRecordingConfig>(AssetPath);
            if (existing != null) {
                Selection.activeObject = existing;
                Debug.Log("AdRecordingConfig already exists at " + AssetPath);
                return;
            }

            var config = ScriptableObject.CreateInstance<AdRecordingConfig>();
            AssetDatabase.CreateAsset(config, AssetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = config;
            Debug.Log("AdRecordingConfig created at " + AssetPath);
        }
    }
}
