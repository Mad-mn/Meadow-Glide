using System;
using Feature.StripsModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleCompleteTrackService : ICircleCompleteTrackService, IInitializable, IDisposable {
        private readonly StripModel _stripModel;

        public CircleCompleteTrackService(StripModel stripModel) {
            _stripModel = stripModel;
        }

        public void Initialize() {
            _stripModel.OnSegmentsChanged += HandleSegmentsChanged;
        }

        public void Dispose() {
            _stripModel.OnSegmentsChanged -= HandleSegmentsChanged;
        }

        private void HandleSegmentsChanged() {
            foreach (StripController strip in _stripModel.Strips) {
                if (strip.IsCompleted)
                    _stripModel.ChangeCircleCompleterState(strip, true);
                else
                    _stripModel.ChangeCircleCompleterState(strip, false);
            }
        }
    }
}
