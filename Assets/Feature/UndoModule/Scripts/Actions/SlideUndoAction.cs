using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Feature.SlideAreaModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;

namespace Feature.UndoModule.Scripts.Actions
{
    public struct SegmentRestoreData
    {
        public StripSegment Segment;
        public StripController OriginalStrip;
        public StripController SourceStrip;
        public int OriginalIndex;
    }

    public class SlideUndoAction : IUndoableAction
    {
        private readonly List<SegmentRestoreData> _segmentData;
        private readonly bool _consumedMove;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly StripModel _stripModel;
        private readonly ISlideSegmentService _slideSegmentService;

        public SlideUndoAction(
            List<SegmentRestoreData> segmentData,
            bool consumedMove,
            MoveTrackModel moveTrackModel,
            StripModel stripModel,
            ISlideSegmentService slideSegmentService)
        {
            _segmentData = segmentData;
            _consumedMove = consumedMove;
            _moveTrackModel = moveTrackModel;
            _stripModel = stripModel;
            _slideSegmentService = slideSegmentService;
        }

        public async UniTask ExecuteReverse()
        {
            await UniTask.Yield();
        }

        public void RestoreState()
        {
            foreach (var data in _segmentData)
            {
                data.SourceStrip.RemoveSegment(data.Segment);
            }

            foreach (var data in _segmentData)
            {
                data.OriginalStrip.AddSegment(data.Segment, data.OriginalIndex);
            }

            _stripModel.SegmentsChanged();
            _slideSegmentService.UpdateSegmentsInAreas();

            if (_consumedMove)
                _moveTrackModel.AddMoves(1);
        }
    }
}
