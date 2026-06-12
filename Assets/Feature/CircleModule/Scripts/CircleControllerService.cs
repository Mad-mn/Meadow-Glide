using System;
using System.Collections.Generic;
using Feature.LoseViewModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleControllerService : ICircleControllerService, IInitializable, IDisposable {
        private readonly StripModel _stripModel;
        private readonly IViewService _viewService;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly MoveTrackModel _moveTrackModel;

        private List<StripController> _filledStrips = new List<StripController>();

        private bool _isWin;
        private bool _isLose;

        public CircleControllerService(StripModel stripModel, IViewService viewService,
            ISaveDataModel saveDataModel, ISaveDataService saveDataService, MoveTrackModel moveTrackModel) {
            _stripModel = stripModel;
            _viewService = viewService;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _moveTrackModel = moveTrackModel;
        }

        public void Initialize() {
            _stripModel.OnStripCompletedStatusChanged += OnStripCompletedStatusChanged;
        }

        public void Dispose() {
            _stripModel.OnStripCompletedStatusChanged -= OnStripCompletedStatusChanged;
        }

        private void OnStripCompletedStatusChanged(StripController strip, bool isCompleted) {
            ApplyResultForStrip(strip, isCompleted);
            if (isCompleted)
                strip.PlayCompletedAnimation(CheckForMatchResult);
        }

        public void Reset() {
            _filledStrips.Clear();
            _isWin = false;
            _isLose = false;
        }

        private void CheckForMatchResult() {
            if (!CheckForWin())
                CheckForLose();
        }

        private bool CheckForWin() {
            if (_filledStrips.Count != _stripModel.Strips.Count || _stripModel.Strips.Count <= 0)
                return false;

            ApplyWin();
            return true;
        }

        private void CheckForLose() {
            if (_moveTrackModel.MovesLeft == 0) {
                if (_isLose)
                    return;

                _isLose = true;
                _viewService.ShowView<LoseView>(ViewType.LoseView);
            }
        }

        private void ApplyWin() {
            if (_isWin)
                return;

            _isWin = true;
            _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress)
                .Level++;
            _saveDataService.Save(SaveDataType.PlayerProgress);
            _viewService.ShowView<WinLevel>(ViewType.WinLevel);
        }

        private void ApplyResultForStrip(StripController strip, bool stripFull) {
            if (stripFull) {
                if (_filledStrips.Contains(strip))
                    return;

                _filledStrips.Add(strip);
            }
            else {
                if (!_filledStrips.Contains(strip))
                    return;

                _filledStrips.Remove(strip);
            }
        }
    }
}
