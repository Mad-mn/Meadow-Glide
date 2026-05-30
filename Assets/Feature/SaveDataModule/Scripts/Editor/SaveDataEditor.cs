using UnityEditor;
using UnityEngine;
using System.IO;

namespace Feature.SaveDataModule.Scripts.Editor
{
    public static class SaveDataEditor
    {
        [MenuItem("Tools/Save Data/Clear All Saves")]
        public static void ClearAllSaves()
        {
            string[] files = Directory.GetFiles(Application.persistentDataPath, "*.dat");
            foreach (string file in files)
            {
                File.Delete(file);
            }
            Debug.Log("All save files cleared.");
        }
        
        [MenuItem("Tools/Save Data/Open Persistent Data Path")]
        public static void OpenPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }
    }
}