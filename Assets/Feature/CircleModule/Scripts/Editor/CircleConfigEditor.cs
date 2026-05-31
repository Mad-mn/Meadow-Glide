using System.Collections.Generic;
using Feature.ColorServiceModule.Scripts;
using UnityEditor;
using UnityEngine;

namespace Feature.CircleModule.Scripts.Editor
{
    [CustomEditor(typeof(CircleConfig))]
    public class CircleConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw all default fields (segmentCount, radius, segments list)
            DrawDefaultInspector();

            CircleConfig config = (CircleConfig)target;

            EditorGUILayout.Space();
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button("Generate Segments", GUILayout.Height(30)))
            {
                GenerateSegments(config);
            }
            GUI.backgroundColor = Color.white;
        }

        private void GenerateSegments(CircleConfig config)
        {
            // Register for Undo
            Undo.RecordObject(config, "Generate Circle Segments");

            // Logic: Clear and refill the list
            if (config.Segments == null)
            {
                config.Segments = new List<SegmentConfig>();
            }
            else
            {
                config.Segments.Clear();
            }

            if (config.SegmentCount <= 0)
            {
                Debug.LogWarning("Segment count must be greater than 0!");
                return;
            }

            float anglePerSegment = 360f / config.SegmentCount;

            for (int i = 0; i < config.SegmentCount; i++)
            {
                config.Segments.Add(new SegmentConfig
                {
                    Angle = anglePerSegment,
                    ColorType = CircleColorType.White
                });
            }

            // Mark as dirty so Unity knows it needs to be saved
            EditorUtility.SetDirty(config);
            // Save the asset changes to disk
            AssetDatabase.SaveAssets();
            
            Debug.Log($"Generated {config.SegmentCount} segments for {config.name}");
        }
    }
}
