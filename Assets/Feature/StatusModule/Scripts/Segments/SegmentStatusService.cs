using System.Linq;
using Feature.CircleModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.StatusModule.Scripts;
using Feature.StripsModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.StatusModule.Scripts.Segments {
    public class SegmentStatusService : ISegmentStatusService, IInitializable {
        private readonly StripModel _stripModel;
        private readonly SlideAreaModel _slideAreaModel;

        public SegmentStatusService(StripModel stripModel, SlideAreaModel slideAreaModel) {
            _stripModel = stripModel;
            _slideAreaModel = slideAreaModel;
        }

        public void Initialize() {
            _stripModel.OnStripRotationStatusChanged += HandleRotationStatusChanged;
        }

        private void HandleRotationStatusChanged(StripController strip, bool status) {
            if (!status)
                UpdateStatus();
        }

        public void UpdateStatus() {
            foreach (StripController strip in _stripModel.Strips) {
                foreach (StripSegment segment in strip.SpawnedSegments) {
                    bool isInArea = _slideAreaModel.SegmentsInAreas.Contains(segment);
                    SegmentStatus currentStatus = segment.GetStatus();
                    if (isInArea) {
                        if (currentStatus is SegmentStatus.Default or SegmentStatus.Horizontal)
                            segment.SetStatus(SegmentStatus.Vertical);
                    }
                    else {
                        if (currentStatus is SegmentStatus.Default or SegmentStatus.Vertical)
                            segment.SetStatus(SegmentStatus.Horizontal);
                    }
                }
            }
        }
    }
}
