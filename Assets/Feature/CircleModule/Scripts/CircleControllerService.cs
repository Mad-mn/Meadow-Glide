using System;
using System.Collections.Generic;
using Feature.ColorServiceModule.Scripts;
using Feature.LoseViewModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleControllerService : ICircleControllerService, IInitializable, IDisposable {
        private readonly GameCircleModel _circleModel;
        private readonly IViewService _viewService;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly MoveTrackModel _moveTrackModel;

        private List<CircleController> _filledCircles = new List<CircleController>();

        public CircleControllerService(GameCircleModel circleModel, IViewService viewService,
            ISaveDataModel saveDataModel, ISaveDataService saveDataService, MoveTrackModel moveTrackModel) {
            _circleModel = circleModel;
            _viewService = viewService;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _moveTrackModel = moveTrackModel;
        }

        public void Initialize() {
            _moveTrackModel.OnMovesChanged += OnCircleSegmentChanged;
        }

        public void Dispose() {
            _moveTrackModel.OnMovesChanged -= OnCircleSegmentChanged;
        }

        public void Reset() {
            _filledCircles.Clear();
        }

        private void OnCircleSegmentChanged() {
            CheckCheckForMatchColors();
        }

        private void CheckCheckForMatchColors() {
            UpdateCirclesStates();
            if(!CheckForWin())
                CheckForLose();
        }

        private bool CheckForWin() {
            if (_filledCircles.Count != _circleModel.Circles.Count || _circleModel.Circles.Count <= 0)
                return false;

            ApplyWin();
            return true;
        }
        
        private void CheckForLose() {
            if (_moveTrackModel.MovesLeft == 0) {
                _viewService.ShowView<LoseView>(ViewType.LoseView);
            }
        }

        private void ApplyWin() {
            _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress)
                .Level++;
            _saveDataService.Save(SaveDataType.PlayerProgress);
            _viewService.ShowView<WinLevel>(ViewType.WinLevel);
            
        }

        private void UpdateCirclesStates() {
            foreach (CircleController circle in _circleModel.Circles) {
                bool circleFull = IsCircleUniform(circle);
                ApplyResultForCircle(circle, circleFull);
            }
        }

        private bool IsCircleUniform(CircleController circle) {
            if (circle.SpawnedSegments.Count == 0 || circle.SpawnedSegments.Count != circle.SegmentCount) {
                return false;
            }

            CircleColorType targetColor = CircleColorType.None;

            foreach (CircleSegment segment in circle.SpawnedSegments) {
                if (targetColor == CircleColorType.None) {
                    targetColor = segment.ColorType;
                    
                    // None or White usually shouldn't count as a completed circle color
                    if (targetColor == CircleColorType.None) {
                        return false;
                    }
                    continue;
                }

                if (segment.ColorType != targetColor) {
                    return false;
                }
            }

            return true;
        }

        private void ApplyResultForCircle(CircleController circle, bool circleFull) {
            if (circleFull) {
                if(_filledCircles.Contains(circle))
                    return;

                _filledCircles.Add(circle);
            }
            else {
                if(!_filledCircles.Contains(circle))
                    return;

                _filledCircles.Remove(circle);
            }
        }
    }
}