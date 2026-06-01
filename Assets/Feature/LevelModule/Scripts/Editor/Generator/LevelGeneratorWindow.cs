using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.LevelModule.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Feature.LevelModule.Scripts.Editor.Generator {
    public class LevelGeneratorWindow : EditorWindow {
        private LevelGenerator _generator;
        private LevelData _currentLevel;
        private VisualElement _previewArea;
        private Label _statsLabel;
        private Button _generateButton;

        [MenuItem("Tools/ColorRings/Level Generator")]
        public static void ShowWindow() {
            GetWindow<LevelGeneratorWindow>("Level Generator");
        }

        private void CreateGUI() {
            _generator = new LevelGenerator();

            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Feature/LevelModule/Scripts/Editor/Generator/LevelGeneratorWindow.uxml");
            rootVisualElement.Add(uxml.Instantiate());

            _previewArea = rootVisualElement.Q<VisualElement>("previewArea");
            _statsLabel = rootVisualElement.Q<Label>("statsLabel");
            _generateButton = rootVisualElement.Q<Button>("generateButton");

            _previewArea.generateVisualContent += DrawPreview;

            _generateButton.clicked += OnGenerateClicked;
            rootVisualElement.Q<Button>("saveButton").clicked += OnSaveClicked;
        }

        private async void OnGenerateClicked() {
            _generateButton.SetEnabled(false);
            _statsLabel.text = "Generating... (Solving with A*)";
            
            var p = new LevelGenerator.GenerationParams {
                MinRings = rootVisualElement.Q<IntegerField>("minRings").value,
                MaxRings = rootVisualElement.Q<IntegerField>("maxRings").value,
                MinAreas = rootVisualElement.Q<IntegerField>("minAreas").value,
                MaxAreas = rootVisualElement.Q<IntegerField>("maxAreas").value,
                MinSectors = rootVisualElement.Q<IntegerField>("minSectors").value,
                MaxSectors = rootVisualElement.Q<IntegerField>("maxSectors").value,
                AllowBlocked = rootVisualElement.Q<Toggle>("allowBlocked").value,
                BlockedChance = rootVisualElement.Q<Slider>("blockedChance").value,
                AllowFilterColors = rootVisualElement.Q<Toggle>("allowFilterColors").value,
                FilterColorsChance = rootVisualElement.Q<Slider>("filterChance").value
            };

            int targetDepth = rootVisualElement.Q<IntegerField>("targetDifficulty").value;

            try {
                var rawData = await _generator.GenerateAsync(p, targetDepth);

                if (rawData != null) {
                    _currentLevel = ConvertToUnityLevelData(rawData);
                    _statsLabel.text = $"Difficulty: {_currentLevel.Difficulty} | Rings: {_currentLevel.LevelConfig.CircleConfigs.Count}";
                    _previewArea.MarkDirtyRepaint();
                } else {
                    _statsLabel.text = "Timeout or Limit reached. Try again.";
                }
            } catch (Exception e) {
                Debug.LogException(e);
                _statsLabel.text = "Error during generation.";
            } finally {
                _generateButton.SetEnabled(true);
            }
        }

        private LevelData ConvertToUnityLevelData(LevelGenerator.RawLevelData raw) {
            var levelConfig = ScriptableObject.CreateInstance<LevelConfig>();
            var circles = new List<CircleConfig>();

            for (int r = 0; r < raw.Rings; r++) {
                var circle = ScriptableObject.CreateInstance<CircleConfig>();
                circle.SegmentCount = raw.Sectors;
                circle.name = $"Circle_{r}";
                for (int s = 0; s < raw.Sectors; s++) {
                    circle.Segments.Add(new SegmentConfig {
                        ColorType = (Feature.ColorServiceModule.Scripts.CircleColorType)raw.Colors[r, s],
                        SegmentStatus = raw.Statuses[r, s]
                    });
                }
                circles.Add(circle);
            }

            levelConfig.SetConfigs(circles, raw.Areas);

            return new LevelData {
                LevelConfig = levelConfig,
                Difficulty = raw.Difficulty,
                MinimumMoves = raw.Difficulty
            };
        }

        private void OnSaveClicked() {
            if (_currentLevel == null) return;

            string path = EditorUtility.SaveFilePanelInProject("Save Level Config", "LevelConfig_New", "asset", "Save Level Config");
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(_currentLevel.LevelConfig, path);
            foreach (var circle in _currentLevel.LevelConfig.CircleConfigs) {
                AssetDatabase.AddObjectToAsset(circle, _currentLevel.LevelConfig);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", "Level saved to " + path, "OK");
        }

        private void DrawPreview(MeshGenerationContext mgc) {
            if (_currentLevel == null) return;

            var painter = mgc.painter2D;
            var center = _previewArea.contentRect.center;
            float maxRadius = Mathf.Min(_previewArea.contentRect.width, _previewArea.contentRect.height) / 2f - 20f;

            var circles = _currentLevel.LevelConfig.CircleConfigs;
            int count = circles.Count;
            float ringThickness = maxRadius / (count + 1);

            // Draw Areas background
            foreach (var area in _currentLevel.LevelConfig.SlideAreaConfigs) {
                float innerR = (area.startCircleIndex + 1) * ringThickness - ringThickness/2f;
                float outerR = (area.endCircleIndex + 1) * ringThickness + ringThickness/2f;
                int sectors = circles[0].SegmentCount;
                float angleStep = 360f / sectors;
                float startA = area.sectorIndex * angleStep - 90f;
                float endA = (area.sectorIndex + 1) * angleStep - 90f;

                if (area.SlideAreaStatus == Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors) {
                    painter.fillColor = new Color(1, 0.6f, 0, 0.3f);
                    painter.strokeColor = Color.orange;
                    painter.lineWidth = 2f;
                } else {
                    painter.fillColor = new Color(1, 1, 1, 0.15f);
                }

                painter.BeginPath();
                painter.Arc(center, outerR, Angle.Degrees(startA), Angle.Degrees(endA), ArcDirection.Clockwise);
                painter.Arc(center, innerR, Angle.Degrees(endA), Angle.Degrees(startA), ArcDirection.CounterClockwise);
                painter.Fill();
                if (area.SlideAreaStatus == Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors) painter.Stroke();
                
                // Draw sector index
                float midA = (startA + endA) / 2f;
                Vector2 labelPos = center + new Vector2(Mathf.Cos(midA * Mathf.Deg2Rad), Mathf.Sin(midA * Mathf.Deg2Rad)) * (outerR + 15f);
                // Note: Painter2D doesn't support text easily, so we just use small circles or markers if needed.
                // But we can skip text for now as it's not critical for the logic.
            }

            for (int i = 0; i < count; i++) {
                float r = (i + 1) * ringThickness;
                int sectors = circles[i].SegmentCount;
                float angleStep = 360f / sectors;

                for (int s = 0; s < sectors; s++) {
                    var segment = circles[i].Segments[s];
                    painter.strokeColor = GetColor(segment.ColorType);
                    painter.lineWidth = ringThickness * 0.7f;
                    float startA = s * angleStep - 88f;
                    float endA = (s + 1) * angleStep - 92f;

                    painter.BeginPath();
                    painter.Arc(center, r, Angle.Degrees(startA), Angle.Degrees(endA), ArcDirection.Clockwise);
                    painter.Stroke();

                    if (segment.SegmentStatus == Feature.StatusModule.Scripts.Segments.SegmentStatus.Blocked) {
                        painter.fillColor = Color.black;
                        painter.strokeColor = Color.white;
                        painter.lineWidth = 1f;
                        float mid = (startA + endA) / 2f;
                        Vector2 pos = center + new Vector2(Mathf.Cos(mid * Mathf.Deg2Rad), Mathf.Sin(mid * Mathf.Deg2Rad)) * r;
                        painter.BeginPath();
                        painter.Arc(pos, 5f, Angle.Degrees(0), Angle.Degrees(360), ArcDirection.Clockwise);
                        painter.Fill();
                        painter.Stroke();
                    }
                }
            }
}

        private Color GetColor(Feature.ColorServiceModule.Scripts.CircleColorType type) {
            switch (type) {
                case Feature.ColorServiceModule.Scripts.CircleColorType.Red: return Color.red;
                case Feature.ColorServiceModule.Scripts.CircleColorType.Blue: return Color.blue;
                case Feature.ColorServiceModule.Scripts.CircleColorType.Green: return Color.green;
                case Feature.ColorServiceModule.Scripts.CircleColorType.Yellow: return Color.yellow;
                case Feature.ColorServiceModule.Scripts.CircleColorType.Cyan: return Color.cyan;
                case Feature.ColorServiceModule.Scripts.CircleColorType.Magenta: return Color.magenta;
                default: return Color.white;
            }
        }
    }
}
