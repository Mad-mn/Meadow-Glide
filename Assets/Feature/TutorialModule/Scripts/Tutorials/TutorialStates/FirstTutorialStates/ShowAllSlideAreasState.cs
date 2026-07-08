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
using UnityEngine;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowAllSlideAreasState : ITutorialState {
        private const int SEGMENT_HIGHLIGHT_ORDER = 10000;

        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;
        private readonly IInputService _inputService;
        private readonly StripModel _stripModel;
        private readonly SlideAreaModel _slideAreaModel;
        private readonly IInteractionStateService _interactionState;
        private readonly ISlideSegmentService _slideSegmentService;

        public event Action OnComplete;

        private bool _isTapped;
        private readonly List<(StripSegment segment, int originalOrder)> _highlightedSegments = new();
        private readonly List<(SlideArea area, float originalScale)> _highlightedAreas = new();

        public ShowAllSlideAreasState(
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            IInputService inputService,
            StripModel stripModel,
            SlideAreaModel slideAreaModel,
            IInteractionStateService interactionState,
            ISlideSegmentService slideSegmentService) {
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _inputService = inputService;
            _stripModel = stripModel;
            _slideAreaModel = slideAreaModel;
            _interactionState = interactionState;
            _slideSegmentService = slideSegmentService;
        }

        public void Enter() {
            _isTapped = false;
            _tutorialViewModel.RequestText(LocalizationKey.Tutorial_SlideArea);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
            _interactionState.BlockInput();
            _slideSegmentService.UpdateSegmentsInAreas();
            HighlightAllSlideAreaSegments();
            //HighlightSlideAreaObjects();
            _inputService.PointerDown += HandlePointerDown;
        }

        private void HighlightAllSlideAreaSegments() {
            _highlightedSegments.Clear();
            foreach (StripController strip in _stripModel.Strips) {
                foreach (StripSegment segment in strip.SpawnedSegments) {
                    if (_slideAreaModel.SegmentsInAreas.Contains(segment)) {
                        int original = segment.GetSortingOrder();
                        _highlightedSegments.Add((segment, original));
                        segment.SetSortingOrder(SEGMENT_HIGHLIGHT_ORDER);
                    }
                }
            }
        }

        private void HighlightSlideAreaObjects() {
            _highlightedAreas.Clear();
            foreach (SlideArea area in _slideAreaModel.SpawnedAreas) {
                float originalScale = area.transform.localScale.x;
                _highlightedAreas.Add((area, originalScale));
                area.transform.localScale = Vector3.one * 1.05f;
            }
        }

        private void RestoreAll() {
            foreach (var (segment, originalOrder) in _highlightedSegments) {
                if (segment != null)
                    segment.SetSortingOrder(originalOrder);
            }
            _highlightedSegments.Clear();

            foreach (var (area, originalScale) in _highlightedAreas) {
                if (area != null)
                    area.transform.localScale = new Vector3(originalScale, originalScale, 1f);
            }
            _highlightedAreas.Clear();
        }

        private void HandlePointerDown() {
            if (_isTapped) return;
            _isTapped = true;
            _inputService.PointerDown -= HandlePointerDown;
            OnComplete?.Invoke();
        }

        public void Exit() {
            _inputService.PointerDown -= HandlePointerDown;
            _viewService.HideView(ViewType.TutorialView);
            _interactionState.UnblockInput();
            RestoreAll();
        }
    }
}
