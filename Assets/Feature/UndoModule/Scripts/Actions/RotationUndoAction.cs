using Cysharp.Threading.Tasks;
using Feature.SlideAreaModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using UnityEngine;

namespace Feature.UndoModule.Scripts.Actions
{
    public class RotationUndoAction : IUndoableAction
    {
        private readonly StripController _strip;
        private readonly float _previousOffset;
        private readonly float _currentOffset;
        private readonly bool _consumedMove;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ISlideSegmentService _slideSegmentService;
        private readonly StripModel _stripModel;

        public RotationUndoAction(
            StripController strip,
            float previousOffset,
            float currentOffset,
            bool consumedMove,
            MoveTrackModel moveTrackModel,
            ISlideSegmentService slideSegmentService,
            StripModel stripModel)
        {
            _strip = strip;
            _previousOffset = previousOffset;
            _currentOffset = currentOffset;
            _consumedMove = consumedMove;
            _moveTrackModel = moveTrackModel;
            _slideSegmentService = slideSegmentService;
            _stripModel = stripModel;
        }

        public async UniTask ExecuteReverse()
        {
            float duration = 0.25f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3f);
                float offset = Mathf.Lerp(_currentOffset, _previousOffset, t);
                _strip.SetScrollOffset(offset, true);
                await UniTask.Yield();
            }

            _strip.SetScrollOffset(_previousOffset, false);
            _strip.ClearWrapGhosts();
            _slideSegmentService.UpdateSegmentsInAreas();
            _stripModel.CircleRotationStatusChanges(_strip, false);
        }

        public void RestoreState()
        {
            if (_consumedMove)
                _moveTrackModel.AddMoves(1);
        }
    }
}
