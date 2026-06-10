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
using UnityEngine.UIElements;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleControllerService : ICircleControllerService, IInitializable, IDisposable {
        private readonly GameCircleModel _circleModel;
        private readonly IViewService _viewService;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly MoveTrackModel _moveTrackModel;

        private List<CircleController> _filledCircles = new List<CircleController>();

        private bool _isWin;
        private bool _isLose;

        public CircleControllerService(GameCircleModel circleModel, IViewService viewService,
            ISaveDataModel saveDataModel, ISaveDataService saveDataService, MoveTrackModel moveTrackModel) {
            _circleModel = circleModel;
            _viewService = viewService;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _moveTrackModel = moveTrackModel;
        }

        public void Initialize() {
            _circleModel.OnCircleCompletedStatusChanged += OnCircleCompletedStatusChanged;
        }

        public void Dispose() {
            _circleModel.OnCircleCompletedStatusChanged -= OnCircleCompletedStatusChanged;
        }

        private void OnCircleCompletedStatusChanged(CircleController circle, bool isCompleted) {
            ApplyResultForCircle(circle, isCompleted);
            if(isCompleted)
                circle.PlayCompletedAnimation(CheckForMatchResult);
        }

        public void Reset() {
            _filledCircles.Clear();
            _isWin = false;
            _isLose = false;
        }

        private void CheckForMatchResult() {
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
                if(_isLose)
                    return;
                
                _isLose = true;
                _viewService.ShowView<LoseView>(ViewType.LoseView);
            }
        }

        private void ApplyWin() {
            if(_isWin)
                return;
            
            _isWin = true;
            _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress)
                .Level++;
            _saveDataService.Save(SaveDataType.PlayerProgress);
            _viewService.ShowView<WinLevel>(ViewType.WinLevel);
            
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