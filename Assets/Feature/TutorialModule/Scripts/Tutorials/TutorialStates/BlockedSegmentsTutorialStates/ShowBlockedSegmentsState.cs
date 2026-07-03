using System;
using System.Collections.Generic;
using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.StatusModule.Scripts.Segments;
using Feature.StripsModule.Scripts;
using Feature.TutorialViewModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.BlockedSegmentsTutorialStates {
    public class ShowBlockedSegmentsState : ITutorialState {
        private const int HIGHLIGHT_SORTING_ORDER = 10000;

        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;
        private readonly IInputService _inputService;
        private readonly StripModel _stripModel;

        public event Action OnComplete;

        private bool _isTapped;
        private readonly List<(StripSegment segment, int originalOrder)> _highlightedSegments = new();

        public ShowBlockedSegmentsState(
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            IInputService inputService,
            StripModel stripModel) {
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _inputService = inputService;
            _stripModel = stripModel;
        }

        public void Enter() {
            _isTapped = false;
            _tutorialViewModel.RequestText(LocalizationKey.Tutorial_BlockedSegments);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
            HighlightBlockedSegments();
            _inputService.PointerDown += HandlePointerDown;
        }

        private void HighlightBlockedSegments() {
            _highlightedSegments.Clear();

            foreach (StripController strip in _stripModel.Strips) {
                foreach (StripSegment segment in strip.SpawnedSegments) {
                    if (segment.GetStatus() != SegmentStatus.Blocked)
                        continue;

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
