using System;
using System.Collections.Generic;
using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.StripsModule.Scripts;
using Feature.TutorialViewModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowWinConditionState : ITutorialState {
        private const int HIGHLIGHT_SORTING_ORDER = 10000;

        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;
        private readonly IInputService _inputService;
        private readonly StripModel _stripModel;
        private readonly IInteractionStateService _interactionState;

        public event Action OnComplete;

        private bool _isTapped;
        private readonly List<(StripSegment segment, int originalOrder)> _highlightedSegments = new();

        public ShowWinConditionState(
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            IInputService inputService,
            StripModel stripModel,
            IInteractionStateService interactionState) {
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _inputService = inputService;
            _stripModel = stripModel;
            _interactionState = interactionState;
        }

        public void Enter() {
            _isTapped = false;
            _tutorialViewModel.RequestText(LocalizationKey.Tutorial_WinCondition);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
            _interactionState.BlockInput();
            HighlightCompletedStrip();
            _inputService.PointerDown += HandlePointerDown;
        }

        private void HighlightCompletedStrip() {
            _highlightedSegments.Clear();

            foreach (StripController strip in _stripModel.Strips) {
                if (!strip.IsCompleted)
                    continue;

                foreach (StripSegment segment in strip.SpawnedSegments) {
                    int original = segment.GetSortingOrder();
                    _highlightedSegments.Add((segment, original));
                    segment.SetSortingOrder(HIGHLIGHT_SORTING_ORDER);
                }
                break;
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
            _interactionState.UnblockInput();
            RestoreSegments();
        }
    }
}
