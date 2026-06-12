using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.TrackMoveModule.Scripts {
    public class MoveTrackService : IMoveTrackService, IInitializable, IDisposable {
        private readonly SlideAreaModel _slideAreaModel;
        private readonly StripModel _stripModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IViewService _viewService;

        private Dictionary<StripController, float> _stripScrollOffsets = new Dictionary<StripController, float>();
        private Dictionary<IGameSegment, float> _cachedSegmentPositions = new Dictionary<IGameSegment, float>();

        public MoveTrackService(SlideAreaModel slideAreaModel, StripModel stripModel, MoveTrackModel moveTrackModel,
            IViewService viewService) {
            _slideAreaModel = slideAreaModel;
            _stripModel = stripModel;
            _moveTrackModel = moveTrackModel;
            _viewService = viewService;
        }

        public void Initialize() {
            _slideAreaModel.OnChangeSlideState += OnChangeSlideState;
            _stripModel.OnStripRotationStatusChanged += OnStripRotationStatusChanged;
        }

        public void Dispose() {
            _slideAreaModel.OnChangeSlideState -= OnChangeSlideState;
            _stripModel.OnStripRotationStatusChanged -= OnStripRotationStatusChanged;
        }

        private void OnStripRotationStatusChanged(StripController strip, bool isRotating) {
            if (isRotating) {
                _stripScrollOffsets[strip] = strip.ScrollOffset;
            }
            else {
                CheckForSpendByRotation(strip);
            }
        }

        private void OnChangeSlideState(bool slideState) {
            if (slideState) {
                _cachedSegmentPositions = GetSegmentPositions();
            }
            else {
                CheckForSpendStepBySlide();
            }
        }

        private void CheckForSpendByRotation(StripController strip) {
            if (!_stripScrollOffsets.TryGetValue(strip, out float startOffset))
                return;

            if (Mathf.Abs(strip.ScrollOffset - startOffset) > 0.01f)
                _moveTrackModel.Move();

            _stripScrollOffsets.Remove(strip);
        }

        private void CheckForSpendStepBySlide() {
            if (_cachedSegmentPositions.Count == 0)
                return;

            Dictionary<IGameSegment, float> updated = GetSegmentPositions();
            foreach (KeyValuePair<IGameSegment, float> cached in _cachedSegmentPositions) {
                if (!updated.TryGetValue(cached.Key, out float updatedPosition))
                    continue;

                if (!Mathf.Approximately(updatedPosition, cached.Value)) {
                    _moveTrackModel.Move();
                    break;
                }
            }

            _cachedSegmentPositions.Clear();
        }

        private Dictionary<IGameSegment, float> GetSegmentPositions() {
            Dictionary<IGameSegment, float> current = new Dictionary<IGameSegment, float>();
            foreach (IGameSegment segment in _slideAreaModel.ActiveSegments) {
                current.Add(segment, GetSegmentTrackPosition(segment));
            }

            return current;
        }

        private static float GetSegmentTrackPosition(IGameSegment segment) {
            if (segment is StripSegment stripSegment) {
                StripController strip = stripSegment.GetComponentInParent<StripController>();
                return strip != null ? strip.PositionIndex : stripSegment.Radius;
            }

            return segment.Radius;
        }

        public void AddMoves(int addedMoves) {
            _moveTrackModel.AddMoves(addedMoves);
        }
    }
}
