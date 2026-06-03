using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideAreaModel {
        private readonly List<CircleSegment> _activeSegments = new List<CircleSegment>();
        private readonly HashSet<CircleSegment> _segmentsInAreas = new HashSet<CircleSegment>();
        
        public IReadOnlyList<CircleSegment> ActiveSegments => _activeSegments;
        public IReadOnlyCollection<CircleSegment> SegmentsInAreas => _segmentsInAreas;

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

        public void UpdateSegmentsInAreas(IEnumerable<CircleSegment> segments) {
            _segmentsInAreas.Clear();
            foreach (var segment in segments) {
                _segmentsInAreas.Add(segment);
            }
        }
    }
}