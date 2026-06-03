using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using UnityEngine;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideAreaModel {
        private readonly List<CircleSegment> _activeSegments = new List<CircleSegment>();
        private readonly HashSet<CircleSegment> _segmentsInAreas = new HashSet<CircleSegment>();
        private List<SlideArea> _spawnedAreas = new List<SlideArea>();

        public IReadOnlyList<SlideArea> SpawnedAreas => _spawnedAreas;
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

        public void SetupAreas(List<SlideArea> spawnedAreas) {
            _spawnedAreas.Clear();
            _spawnedAreas.AddRange(spawnedAreas);
        }
    }
}