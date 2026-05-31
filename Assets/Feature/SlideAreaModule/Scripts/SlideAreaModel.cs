using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideAreaModel {
        private readonly List<CircleSegment> _activeSegments = new List<CircleSegment>();
        
        public IReadOnlyList<CircleSegment> ActiveSegments => _activeSegments;
        public event Action<bool> OnChangeSlideState;
        
        public bool IsSliding { get; private set; }

        public void ChangeSlideState(bool state) {
            IsSliding = state;
            OnChangeSlideState?.Invoke(state);
        }

        public void SetupActiveSegments(List<CircleSegment> segments) {
            _activeSegments.Clear();
            _activeSegments.AddRange(segments);
        }
    }
}