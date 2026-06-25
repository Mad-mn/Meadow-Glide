using System;
using System.Collections.Generic;
using Feature.LevelResultModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleControllerService : ICircleControllerService, IInitializable, IDisposable {
        private readonly StripModel _stripModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ILevelResultService _levelResultService;

        private List<StripController> _filledStrips = new List<StripController>();

        private bool _isWin;
        private bool _isLose;

        public CircleControllerService(StripModel stripModel, MoveTrackModel moveTrackModel,
            ILevelResultService levelResultService) {
            _stripModel = stripModel;
            _moveTrackModel = moveTrackModel;
            _levelResultService = levelResultService;
        }

        public void Initialize() {
            _stripModel.OnStripCompletedStatusChanged += OnStripCompletedStatusChanged;
            _moveTrackModel.OnMovesChanged += OnMovesChanged;
        }

        public void Dispose() {
            _stripModel.OnStripCompletedStatusChanged -= OnStripCompletedStatusChanged;
            _moveTrackModel.OnMovesChanged -= OnMovesChanged;
        }

        private void OnMovesChanged() {
            if (!_isWin)
                CheckForLose();
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
            _levelResultService.Reset();
        }

        private void CheckForMatchResult() {
            if (!CheckForWin())
                CheckForLose();
        }

        private bool CheckForWin() {
            if (_filledStrips.Count != _stripModel.Strips.Count || _stripModel.Strips.Count <= 0)
                return false;

            _levelResultService.OnLevelWon();
            _isWin = true;
            return true;
        }

        private void CheckForLose() {
            if (_moveTrackModel.MovesLeft == 0) {
                if (_isLose)
                    return;

                _isLose = true;
                _levelResultService.OnLevelLost();
            }
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
