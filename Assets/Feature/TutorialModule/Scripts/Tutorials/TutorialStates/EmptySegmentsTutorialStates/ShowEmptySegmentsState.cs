using System;
using System.Collections.Generic;
using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.PreGamePlacementModule.Scripts;
using Feature.StatusModule.Scripts.Segments;
using Feature.StripsModule.Scripts;
using Feature.TutorialViewModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.EmptySegmentsTutorialStates {
    public class ShowEmptySegmentsState : ITutorialState {
        private const int HIGHLIGHT_SORTING_ORDER = 10000;

        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;
        private readonly IInputService _inputService;
        private readonly StripModel _stripModel;
        private readonly IPreGamePlacementService _preGamePlacementService;

        public event Action OnComplete;

        private bool _isTapped;
        private readonly List<(StripSegment segment, int originalOrder)> _highlightedSegments = new();

        public ShowEmptySegmentsState(
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            IInputService inputService,
            StripModel stripModel,
            IPreGamePlacementService preGamePlacementService) {
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _inputService = inputService;
            _stripModel = stripModel;
            _preGamePlacementService = preGamePlacementService;
        }

        public void Enter() {
            _isTapped = false;
            _tutorialViewModel.RequestText(LocalizationKey.Tutorial_EmptySegments);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
            HighlightSegments();
            _inputService.PointerDown += HandlePointerDown;
        }

        private void HighlightSegments() {
            _highlightedSegments.Clear();

            foreach (StripController strip in _stripModel.Strips) {
                foreach (StripSegment segment in strip.SpawnedSegments) {
                    if (segment.GetStatus() != SegmentStatus.Empty)
                        continue;

                    int original = segment.GetSortingOrder();
                    _highlightedSegments.Add((segment, original));
                    segment.SetSortingOrder(HIGHLIGHT_SORTING_ORDER);
                }
            }

            foreach (StripController poolPiece in _preGamePlacementService.GetPoolPieces()) {
                foreach (StripSegment segment in poolPiece.SpawnedSegments) {
                    int original = segment.GetSortingOrder();
                    _highlightedSegments.Add((segment, original));
                    segment.SetSortingOrder(HIGHLIGHT_SORTING_ORDER);
                }
            }
        }

        private void RestoreSegments() {
            foreach (var (segment, originalOrder) in _highlightedSegments) {
                if (segment != null)
                    segment.SetSortingOrder(originalOrder);
            }
            _highlightedSegments.Clear();
        }

        private void HandlePointerDown() {
            if (_isTapped)
                return;

            _isTapped = true;
            _inputService.PointerDown -= HandlePointerDown;
            OnComplete?.Invoke();
        }

        public void Exit() {
            _inputService.PointerDown -= HandlePointerDown;
            _viewService.HideView(ViewType.TutorialView);
            RestoreSegments();
        }
    }
}
