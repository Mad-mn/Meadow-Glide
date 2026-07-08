using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideAreaModel {
        private readonly List<IGameSegment> _activeSegments = new List<IGameSegment>();
        private readonly HashSet<IGameSegment> _segmentsInAreas = new HashSet<IGameSegment>();
        private List<SlideArea> _spawnedAreas = new List<SlideArea>();

        public IReadOnlyList<SlideArea> SpawnedAreas => _spawnedAreas;
        public IReadOnlyList<IGameSegment> ActiveSegments => _activeSegments;
        public IReadOnlyCollection<IGameSegment> SegmentsInAreas => _segmentsInAreas;

        public event Action<bool> OnChangeSlideState;

        public bool IsSliding { get; private set; }

        public void ChangeSlideState(bool state) {
            IsSliding = state;
            OnChangeSlideState?.Invoke(state);
        }

        public void SetupActiveSegments<T>(List<T> segments) where T : IGameSegment {
            _activeSegments.Clear();
            foreach (T segment in segments)
                _activeSegments.Add(segment);
        }

        public void UpdateSegmentsInAreas(IEnumerable<IGameSegment> segments) {
            _segmentsInAreas.Clear();
            foreach (IGameSegment segment in segments)
                _segmentsInAreas.Add(segment);
        }

        public void SetupAreas(List<SlideArea> spawnedAreas) {
            _spawnedAreas.Clear();
            _spawnedAreas.AddRange(spawnedAreas);
        }

        public int GetIndexOfArea(SlideArea area) {
            return _spawnedAreas.IndexOf(area);
        }
    }
}
