using UnityEditor;
using UnityEngine;
using Feature.LevelModule.Scripts;
using Feature.StatusModule.Scripts.SlideAreas;

namespace Feature.LevelModule.Scripts.Editor {
    [CustomPropertyDrawer(typeof(SlideAreaConfig))]
    public class SlideAreaConfigDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginProperty(position, label, property);

            // Draw the foldout for the class
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);

            if (property.isExpanded) {
                EditorGUI.indentLevel++;
                float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                // Get properties
                SerializedProperty startCircleIndex = property.FindPropertyRelative("startCircleIndex");
                SerializedProperty endCircleIndex = property.FindPropertyRelative("endCircleIndex");
                SerializedProperty sectorIndex = property.FindPropertyRelative("sectorIndex");
                SerializedProperty totalSegments = property.FindPropertyRelative("totalSegments");
                SerializedProperty slideAreaStatus = property.FindPropertyRelative("SlideAreaStatus");
                SerializedProperty colors = property.FindPropertyRelative("Colors");

                // Draw properties
                y = DrawProperty(ref position, startCircleIndex, y);
                y = DrawProperty(ref position, endCircleIndex, y);
                y = DrawProperty(ref position, sectorIndex, y);
                y = DrawProperty(ref position, totalSegments, y);
                y = DrawProperty(ref position, slideAreaStatus, y);

                // Conditional draw
                if (slideAreaStatus.enumValueIndex == (int)SlideAreaStatus.FilterColors) {
                    float colorsHeight = EditorGUI.GetPropertyHeight(colors, true);
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, colorsHeight), colors, true);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        private float DrawProperty(ref Rect position, SerializedProperty property, float y) {
            float height = EditorGUI.GetPropertyHeight(property);
            EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), property);
            return y + height + EditorGUIUtility.standardVerticalSpacing;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing; // Foldout

            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("startCircleIndex")) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("endCircleIndex")) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("sectorIndex")) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("totalSegments")) + EditorGUIUtility.standardVerticalSpacing;
            height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("SlideAreaStatus")) + EditorGUIUtility.standardVerticalSpacing;

            SerializedProperty slideAreaStatus = property.FindPropertyRelative("SlideAreaStatus");
            if (slideAreaStatus.enumValueIndex == (int)SlideAreaStatus.FilterColors) {
                height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Colors"), true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }
}