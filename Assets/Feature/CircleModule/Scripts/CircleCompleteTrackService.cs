using System;
using UnityEngine;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleCompleteTrackService : ICircleCompleteTrackService, IInitializable, IDisposable {
        private readonly GameCircleModel _circleModel;

        public CircleCompleteTrackService(GameCircleModel circleModel) {
            _circleModel = circleModel;
        }

        public void Initialize() {
            _circleModel.OnSegmentsChanged += HandleSegmentsChanged;
        }

        public void Dispose() {
            _circleModel.OnSegmentsChanged -= HandleSegmentsChanged;
        }

        private void HandleSegmentsChanged() {
            foreach (CircleController circle in _circleModel.Circles) {
                if (circle.IsCompleted) {
                    _circleModel.ChangeCircleCompleterState(circle, true);
                }
                else {
                    _circleModel.ChangeCircleCompleterState(circle, false);
                }
            }
        }
    }
}