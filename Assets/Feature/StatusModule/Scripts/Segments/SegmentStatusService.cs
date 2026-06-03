using System.Collections.Generic;
using System.Linq;
using Feature.CircleModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.StatusModule.Scripts.Segments {
    public class SegmentStatusService : ISegmentStatusService, IInitializable {
        private readonly GameCircleModel _circleModel;
        private readonly SlideAreaModel _slideAreaModel;
        private readonly ISlideAreaService _slideAreaService;

        public SegmentStatusService(GameCircleModel circleModel, SlideAreaModel slideAreaModel, ISlideAreaService slideAreaService) {
            _circleModel = circleModel;
            _slideAreaModel = slideAreaModel;
            _slideAreaService = slideAreaService;
        }

        public void Initialize() {
            _circleModel.OnCircleRotationStatusChanged += HandleRotationStatusChanged;
        }

        private void HandleRotationStatusChanged(CircleController circle, bool status) {
            if (!status) {
                UpdateStatus();
            }
        }

        public void UpdateStatus() {
            foreach (CircleController circle in _circleModel.Circles) {
                foreach (CircleSegment segment in circle.SpawnedSegments) {
                    bool isInArea = _slideAreaModel.SegmentsInAreas.Contains(segment);
                    SegmentStatus currentStatus = segment.GetStatus();

                    if (isInArea) {
                        if (currentStatus is SegmentStatus.Default or SegmentStatus.Horizontal) {
                            segment.SetStatus(SegmentStatus.Vertical);
                        }
                    }
                    else {
                        if (currentStatus is SegmentStatus.Default or SegmentStatus.Vertical) {
                            segment.SetStatus(SegmentStatus.Horizontal);
                        }
                    }
                }
            }
        }

       
    }
}