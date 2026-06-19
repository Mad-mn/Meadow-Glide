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
        private Label _activeSeedLabel;
        private IntegerField _seedField;
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
            _activeSeedLabel = rootVisualElement.Q<Label>("activeSeedLabel");
            _seedField = rootVisualElement.Q<IntegerField>("seed");
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
                        LevelConfig = selected
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
            var progress = new Progress<string>(msg => {
                _progressLabel.text = msg;
                if (msg.StartsWith("Seed: ")) {
                    _activeSeedLabel.text = $"Active seed: {msg.Substring(6)}";
                }
            });

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
                MaxAreaSpan = rootVisualElement.Q<IntegerField>("maxAreaSpan").value,
                AllowEmptySegments = rootVisualElement.Q<Toggle>("allowEmptySegments").value,
                MinEmptySegments = rootVisualElement.Q<IntegerField>("minEmptySegments").value,
                MaxEmptySegments = rootVisualElement.Q<IntegerField>("maxEmptySegments").value,
                UseIntelligentEmpty = rootVisualElement.Q<Toggle>("useIntelligentEmpty").value,
                EmptyMinScore = rootVisualElement.Q<Slider>("emptyMinScore").value,
                EmptyTopKForSolver = rootVisualElement.Q<IntegerField>("emptyTopKForSolver").value,
                MaxAttempts = rootVisualElement.Q<IntegerField>("maxAttempts").value,
                MaxIterations = rootVisualElement.Q<IntegerField>("maxIterations").value,
                Seed = _seedField.value,
                UseFixedSeed = rootVisualElement.Q<Toggle>("useFixedSeed").value
            };

            int targetDepth = rootVisualElement.Q<IntegerField>("targetDifficulty").value;

            try {
                var rawData = await _generator.GenerateAsync(p, targetDepth, _cts.Token, progress);

                if (rawData != null) {
                    _currentLevel = ConvertToUnityLevelData(rawData);
                    _seedField.SetValueWithoutNotify(rawData.Seed);
                    _activeSeedLabel.text = $"Active seed: {rawData.Seed}";
                    _statsLabel.text = $"Difficulty: {rawData.Difficulty} (path:{rawData.PathLength} confusion:{rawData.AvgConfusion:F2} plan:{rawData.AvgPlanningDepth:F2}) | Rings: {rawData.Rings} | Seed: {rawData.Seed}";
                    _previewArea.MarkDirtyRepaint();
                } else {
                    _statsLabel.text = "Generation failed — no valid level produced.";
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
                LevelConfig = levelConfig
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
            Rect rect = _previewArea.contentRect;
            Vector2 center = rect.center;

            var circles = _currentLevel.LevelConfig.CircleConfigs;
            int stripeCount = circles.Count;
            if (stripeCount == 0) return;

            int totalSectors = circles[0].SegmentCount;

            CircleParamsConfig cfg = LoadCircleParamsConfig();
            float segmentWidth = cfg != null ? cfg.GetUniformSegmentThickness() : 0.3f;
            float distanceBetween = cfg != null ? cfg.DistanceBetweenCircles : 1f;
            float spacing = segmentWidth + distanceBetween;
            float stripLoopLength = cfg != null ? cfg.StripLoopLength : 4f * Mathf.PI;

            float segmentSpan = stripLoopLength / totalSectors;
            float totalHeight = (stripeCount - 1) * spacing;

            float availableWidth = rect.width - 40f;
            float availableHeight = rect.height - 40f;
            float scaleX = availableWidth / stripLoopLength;
            float scaleY = availableHeight / (totalHeight + segmentWidth);
            float scale = Mathf.Min(scaleX, scaleY, 45f) * 1.5f;

            float scaledLoopLength = stripLoopLength * scale;
            float scaledSpacing = spacing * scale;
            float scaledSegmentWidth = segmentWidth * scale;
            float scaledSegmentSpan = segmentSpan * scale;

            float originX = center.x - scaledLoopLength * 0.5f;
            float originY = center.y + totalHeight * scale * 0.5f;

            for (int s = 0; s < totalSectors; s++) {
                float sectorCenterX = originX + (s + 0.5f) * scaledSegmentSpan;
                float dotY = originY + scaledSpacing * 0.7f;
                painter.fillColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
                painter.BeginPath();
                painter.Arc(new Vector2(sectorCenterX, dotY), 2f, Angle.Degrees(0), Angle.Degrees(360), ArcDirection.Clockwise);
                painter.Fill();
            }

            for (int i = 0; i < stripeCount; i++) {
                float stripeY = originY - i * scaledSpacing;

                for (int s = 0; s < totalSectors; s++) {
                    var segment = circles[i].Segments[s];
                    float segLeft = originX + s * scaledSegmentSpan;
                    float segRight = segLeft + scaledSegmentSpan;
                    float segTop = stripeY + scaledSegmentWidth * 0.5f;
                    float segBottom = stripeY - scaledSegmentWidth * 0.5f;

                    if (segment.SegmentStatus == Feature.StatusModule.Scripts.Segments.SegmentStatus.Empty) {
                        painter.fillColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                        DrawRect(painter, segLeft, segBottom, segRight, segTop);
                        painter.Fill();

                        painter.strokeColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                        painter.lineWidth = 2f;
                            //painter.strokePattern = new DashPattern(new float[] { 4f, 4f });
                        DrawRect(painter, segLeft, segBottom, segRight, segTop);
                        painter.Stroke();
                        //painter.strokePattern = default;
                    }
                    else {
                        painter.fillColor = GetColor(segment.ColorType);
                        DrawRect(painter, segLeft, segBottom, segRight, segTop);
                        painter.Fill();

                        painter.strokeColor = new Color(0, 0, 0, 0.3f);
                        painter.lineWidth = 1f;
                        DrawRect(painter, segLeft, segBottom, segRight, segTop);
                        painter.Stroke();

                        if (segment.SegmentStatus == Feature.StatusModule.Scripts.Segments.SegmentStatus.Blocked) {
                            float dotX = (segLeft + segRight) / 2f;
                            float dotY = stripeY;
                            painter.fillColor = Color.black;
                            painter.BeginPath();
                            painter.Arc(new Vector2(dotX, dotY), 5f, Angle.Degrees(0), Angle.Degrees(360), ArcDirection.Clockwise);
                            painter.Fill();
                            painter.strokeColor = Color.white;
                            painter.lineWidth = 1f;
                            painter.Stroke();
                        }
                    }
                }
            }

            float stripeBarWidth = scaledSegmentWidth * 0.4f;

            foreach (var area in _currentLevel.LevelConfig.SlideAreaConfigs) {
                float startY = originY - area.startCircleIndex * scaledSpacing;
                float endY = originY - area.endCircleIndex * scaledSpacing;
                float centerX = originX + (area.sectorIndex + 0.5f) * scaledSegmentSpan;
                float barLeft = centerX - stripeBarWidth * 0.5f;
                float barRight = centerX + stripeBarWidth * 0.5f;

                painter.fillColor = new Color(1f, 1f, 1f, 0.7f);
                DrawRect(painter, barLeft, endY, barRight, startY);
                painter.Fill();

                painter.strokeColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                painter.lineWidth = 1f;
                DrawRect(painter, barLeft, endY, barRight, startY);
                painter.Stroke();

                if (area.SlideAreaStatus == Feature.StatusModule.Scripts.SlideAreas.SlideAreaStatus.FilterColors) {
                    if (area.Colors != null && area.Colors.Count > 0) {
                        float midY = (startY + endY) / 2f;
                        float dotRadius = 4f;
                        float dotSpacing = 12f;
                        float dotsStartY = midY + (area.Colors.Count - 1) * dotSpacing * 0.5f;

                        for (int cIdx = 0; cIdx < area.Colors.Count; cIdx++) {
                            float dotY = dotsStartY - cIdx * dotSpacing;
                            Vector2 dotPos = new Vector2(centerX, dotY);
                            painter.fillColor = GetColor(area.Colors[cIdx]);
                            painter.BeginPath();
                            painter.Arc(dotPos, dotRadius, Angle.Degrees(0), Angle.Degrees(360), ArcDirection.Clockwise);
                            painter.Fill();
                            painter.strokeColor = Color.black;
                            painter.lineWidth = 1f;
                            painter.Stroke();
                        }
                    }
                }
            }
        }

        private static void DrawRect(Painter2D painter, float left, float bottom, float right, float top) {
            painter.BeginPath();
            painter.MoveTo(new Vector2(left, bottom));
            painter.LineTo(new Vector2(right, bottom));
            painter.LineTo(new Vector2(right, top));
            painter.LineTo(new Vector2(left, top));
            painter.ClosePath();
        }

        private static CircleParamsConfig _cachedCircleParamsConfig;

        private static CircleParamsConfig LoadCircleParamsConfig() {
            if (_cachedCircleParamsConfig != null) return _cachedCircleParamsConfig;

            var guids = AssetDatabase.FindAssets("t:CircleParamsConfig");
            if (guids.Length > 0) {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _cachedCircleParamsConfig = AssetDatabase.LoadAssetAtPath<CircleParamsConfig>(path);
            }
            return _cachedCircleParamsConfig;
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
