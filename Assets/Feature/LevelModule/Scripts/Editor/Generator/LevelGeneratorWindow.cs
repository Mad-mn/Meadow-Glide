using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        private Label _progressLabel;
        private Button _generateButton;
        private Button _cancelButton;
        private TextField _nameField;
        private ListView _levelList;
        private List<LevelConfig> _existingLevels = new List<LevelConfig>();
        
        private CancellationTokenSource _cts;

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
            _progressLabel = rootVisualElement.Q<Label>("progressLabel");
            _generateButton = rootVisualElement.Q<Button>("generateButton");
            _cancelButton = rootVisualElement.Q<Button>("cancelButton");
            _nameField = rootVisualElement.Q<TextField>("levelName");
            _levelList = rootVisualElement.Q<ListView>("levelList");

            _previewArea.generateVisualContent += DrawPreview;

            _generateButton.clicked += OnGenerateClicked;
            _cancelButton.clicked += OnCancelClicked;
            rootVisualElement.Q<Button>("saveButton").clicked += OnSaveClicked;
            rootVisualElement.Q<Button>("refreshButton").clicked += RefreshLevelList;

            SetupListView();
            RefreshLevelList();
        }

        private void SetupListView() {
            _levelList.makeItem = () => {
                var container = new VisualElement();
                container.AddToClassList("level-item");
                var label = new Label();
                label.name = "name";
                label.AddToClassList("level-item-label");
                var difficulty = new Label();
                difficulty.name = "difficulty";
                difficulty.AddToClassList("level-item-difficulty");
                container.Add(label);
                container.Add(difficulty);
                return container;
            };

            _levelList.bindItem = (element, i) => {
                if (i >= _existingLevels.Count) return;
                var level = _existingLevels[i];
                element.Q<Label>("name").text = level.name;
                element.Q<Label>("difficulty").text = $"Diff: {level.Difficulty}";
            };

            _levelList.onSelectionChange += obj => {
                var selected = obj.FirstOrDefault() as LevelConfig;
                if (selected != null) {
                    _currentLevel = new LevelData {
                        LevelConfig = selected,
                        Difficulty = selected.Difficulty,
                        MinimumMoves = selected.Difficulty
                    };
                    _statsLabel.text = $"Loaded: {selected.name} | Difficulty: {selected.Difficulty}";
                    _previewArea.MarkDirtyRepaint();
                }
            };

            _levelList.itemsSource = _existingLevels;
        }

        private void RefreshLevelList() {
            _existingLevels.Clear();
            var guids = AssetDatabase.FindAssets("t:LevelConfig");
            foreach (var guid in guids) {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var level = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
                if (level != null) _existingLevels.Add(level);
            }
            _levelList.Rebuild();
        }

        private void OnDestroy() {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void OnCancelClicked() {
            _cts?.Cancel();
            _progressLabel.text = "Cancelling...";
            _cancelButton.SetEnabled(false);
        }

        private async void OnGenerateClicked() {
            _generateButton.SetEnabled(false);
            _cancelButton.RemoveFromClassList("hidden");
            _cancelButton.SetEnabled(true);
            
            _statsLabel.text = "Generating...";
            _progressLabel.text = "Starting...";
            
            _cts = new CancellationTokenSource();
            var progress = new Progress<string>(msg => _progressLabel.text = msg);

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
                FilterColorsChance = rootVisualElement.Q<Slider>("filterChance").value,
                MinFilterColors = rootVisualElement.Q<IntegerField>("minFilterColors").value,
                MaxFilterColors = rootVisualElement.Q<IntegerField>("maxFilterColors").value,
                MinAreaSpan = rootVisualElement.Q<IntegerField>("minAreaSpan").value,
                MaxAreaSpan = rootVisualElement.Q<IntegerField>("maxAreaSpan").value
            };

            int targetDepth = rootVisualElement.Q<IntegerField>("targetDifficulty").value;

            try {
                var rawData = await _generator.GenerateAsync(p, targetDepth, _cts.Token, progress);

                if (rawData != null) {
                    _currentLevel = ConvertToUnityLevelData(rawData);
                    _statsLabel.text = $"Difficulty: {_currentLevel.Difficulty} | Rings: {_currentLevel.LevelConfig.CircleConfigs.Count}";
                    _previewArea.MarkDirtyRepaint();
                } else {
                    _statsLabel.text = "Timeout or Limit reached.";
                }
            } catch (OperationCanceledException) {
                _statsLabel.text = "Generation cancelled.";
                _progressLabel.text = "";
            } catch (Exception e) {
                Debug.LogException(e);
                _statsLabel.text = "Error during generation.";
            } finally {
                _generateButton.SetEnabled(true);
                _cancelButton.AddToClassList("hidden");
                _cts?.Dispose();
                _cts = null;
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

            levelConfig.SetConfigs(circles, raw.Areas, raw.Difficulty);

            return new LevelData {
                LevelConfig = levelConfig,
                Difficulty = raw.Difficulty,
                MinimumMoves = raw.Difficulty
            };
        }

        private void OnSaveClicked() {
            if (_currentLevel == null) return;

            string fileName = _nameField.value;
            if (string.IsNullOrEmpty(fileName)) fileName = "LevelConfig_New";

            string path = EditorUtility.SaveFilePanelInProject("Save Level Config", fileName, "asset", "Save Level Config");
            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(_currentLevel.LevelConfig, path);
            foreach (var circle in _currentLevel.LevelConfig.CircleConfigs) {
                AssetDatabase.AddObjectToAsset(circle, _currentLevel.LevelConfig);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            RefreshLevelList();
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
                float innerR = (area.startCircleIndex + 1) * ringThickness - ringThickness / 2f;
                float outerR = (area.endCircleIndex + 1) * ringThickness + ringThickness / 2f;
                
                float angleStep = 360f / area.totalSegments;
                float startA = area.sectorIndex * angleStep - 90f;
                float endA = (area.sectorIndex + 1) * angleStep - 90f;

                if (area.SlideAreaStatus == Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors) {
                    painter.fillColor = new Color(1, 0.6f, 0, 0.25f);
                    painter.strokeColor = Color.orange;
                    painter.lineWidth = 2f;
                } else {
                    painter.fillColor = new Color(1, 1, 1, 0.1f);
                }

                painter.BeginPath();
                painter.Arc(center, outerR, Angle.Degrees(startA), Angle.Degrees(endA), ArcDirection.Clockwise);
                painter.Arc(center, innerR, Angle.Degrees(endA), Angle.Degrees(startA), ArcDirection.CounterClockwise);
                painter.Fill();
                if (area.SlideAreaStatus == Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors) {
                    painter.Stroke();
                    
                    // Draw allowed colors as small dots
                    if (area.Colors != null && area.Colors.Count > 0) {
                        float midA = (startA + endA) / 2f;
                        float dotRadius = 4f;
                        float spacing = 12f;
                        float startDist = (innerR + outerR) / 2f - (area.Colors.Count - 1) * spacing / 2f;
                        
                        for (int cIdx = 0; cIdx < area.Colors.Count; cIdx++) {
                            float dist = startDist + cIdx * spacing;
                            Vector2 dotPos = center + new Vector2(Mathf.Cos(midA * Mathf.Deg2Rad), Mathf.Sin(midA * Mathf.Deg2Rad)) * dist;
                            painter.fillColor = GetColor(area.Colors[cIdx]);
                            painter.BeginPath();
                            painter.Arc(dotPos, dotRadius, Angle.Degrees(0), Angle.Degrees(360), ArcDirection.Clockwise);
                            painter.Fill();
                            // Border for dot
                            painter.strokeColor = Color.black;
                            painter.lineWidth = 1f;
                            painter.Stroke();
                        }
                    }
                }
            }

            // Draw Sector Indices
            int totalSectors = circles[0].SegmentCount;
            float sectorAngleStep = 360f / totalSectors;
            painter.fillColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
            for (int s = 0; s < totalSectors; s++) {
                float angle = s * sectorAngleStep + (sectorAngleStep / 2f) - 90f;
                Vector2 pos = center + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * (maxRadius + 10f);
                painter.BeginPath();
                painter.Arc(pos, 2f, Angle.Degrees(0), Angle.Degrees(360), ArcDirection.Clockwise);
                painter.Fill();
            }

            for (int i = 0; i < count; i++) {
                float r = (i + 1) * ringThickness;
                int sectors = circles[i].SegmentCount;
                float angleStep = 360f / sectors;
                
                // Draw Ring Index Marker (small dot at start of ring)
                Vector2 ringMarkerPos = center + new Vector2(0, -1) * (r + ringThickness * 0.35f);
                painter.fillColor = new Color(1, 1, 1, 0.2f);
                painter.BeginPath();
                painter.Arc(ringMarkerPos, 1.5f, Angle.Degrees(0), Angle.Degrees(360), ArcDirection.Clockwise);
                painter.Fill();

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
