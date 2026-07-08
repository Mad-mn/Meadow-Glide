using System;
using System.Collections.Generic;
using System.Linq;
using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.SlideAreaModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TutorialViewModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowSlideAreaHighlightState : ITutorialState {
        private const int HIGHLIGHT_SORTING_ORDER = 10000;

        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;
        private readonly IInputService _inputService;
        private readonly StripModel _stripModel;
        private readonly SlideAreaModel _slideAreaModel;
        private readonly ILocalizationService _localizationService;

        public event Action OnComplete;

        private bool _isTapped;
        private readonly List<(StripSegment segment, int originalOrder)> _highlightedSegments = new();

        public ShowSlideAreaHighlightState(
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            IInputService inputService,
            StripModel stripModel,
            SlideAreaModel slideAreaModel,
            ILocalizationService localizationService) {
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _inputService = inputService;
            _stripModel = stripModel;
            _slideAreaModel = slideAreaModel;
            _localizationService = localizationService;
        }

        public void Enter() {
            _isTapped = false;
            _tutorialViewModel.RequestText(LocalizationKey.Tutorial_SlideArea);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
            HighlightSegmentsInSlideAreas();
            _inputService.PointerDown += HandlePointerDown;
        }

        private void HighlightSegmentsInSlideAreas() {
            _highlightedSegments.Clear();

            foreach (StripController strip in _stripModel.Strips) {
                foreach (StripSegment segment in strip.SpawnedSegments) {
                    if (!_slideAreaModel.SegmentsInAreas.Contains(segment))
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
