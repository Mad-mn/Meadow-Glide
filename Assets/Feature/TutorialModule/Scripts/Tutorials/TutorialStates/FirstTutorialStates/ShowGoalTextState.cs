using System;
using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.TutorialViewModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowGoalTextState : ITutorialState {
        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;
        private readonly ILocalizationService _localizationService;
        private readonly IInputService _inputService;

        public event Action OnComplete;

        private bool _isTapped;

        public ShowGoalTextState(
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            ILocalizationService localizationService,
            IInputService inputService) {
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _localizationService = localizationService;
            _inputService = inputService;
        }

        public void Enter() {
            _isTapped = false;
            _tutorialViewModel.RequestText(LocalizationKey.Tutorial_Goal);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
            _inputService.PointerDown += HandlePointerDown;
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
        }
    }
}
