using System.Collections.Generic;
using Feature.LocalizationModule.Scripts.Data;
using Feature.TutorialModule.Scripts.Tutorials;
using Feature.TutorialModule.Scripts.Tutorials.TutorialStates.BlockedSegmentsTutorialStates;
using Feature.TutorialModule.Scripts.Tutorials.TutorialStates.EmptySegmentsTutorialStates;
using Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates;
using Feature.TutorialModule.Scripts.Tutorials.TutorialStates.UndoMoveTutorialStates;
using Zenject;

namespace Feature.TutorialModule.Scripts.Factory {
    public class TutorialFactory : ITutorialFactory {
        private readonly DiContainer _container;

        public TutorialFactory(DiContainer container) {
            _container = container;
        }
        public ITutorial CreateTutorial(TutorialType tutorialType){
            switch (tutorialType) {
                case TutorialType.First:
                    return new Tutorial(GetStatesForFirstTutorial());
                case TutorialType.UndoMove:
                    return new Tutorial(GetStatesForUndoMoveTutorial());
                case TutorialType.BlockedSegments:
                    return new Tutorial(GetStatesForBlockedSegmentsTutorial());
                case TutorialType.EmptySegments:
                    return new Tutorial(GetStatesForEmptySegmentsTutorial());
            }

            return null;
        }

        private List<ITutorialState> GetStatesForFirstTutorial() {
            List<ITutorialState> states = new List<ITutorialState>();
            states.Add(_container.Instantiate<GuidedMoveState>(new object[] { 2, 3, LocalizationKey.Tutorial_Rotation }));
            states.Add(_container.Instantiate<ShowAllSlideAreasState>());
            states.Add(_container.Instantiate<ShowSpecificSlideAreaState>(new object[] { 0, LocalizationKey.Tutorial_SlideArea }));
            states.Add(_container.Instantiate<ShowWinConditionState>());
            return states;
        }

        private List<ITutorialState> GetStatesForUndoMoveTutorial() {
            List<ITutorialState> states = new List<ITutorialState>();
            states.Add(_container.Instantiate<ShowUndoMoveTutorialState>());
            return states;
        }

        private List<ITutorialState> GetStatesForBlockedSegmentsTutorial() {
            List<ITutorialState> states = new List<ITutorialState>();
            states.Add(_container.Instantiate<ShowBlockedSegmentsState>());
            return states;
        }

        private List<ITutorialState> GetStatesForEmptySegmentsTutorial() {
            List<ITutorialState> states = new List<ITutorialState>();
            states.Add(_container.Instantiate<ShowEmptySegmentsState>());
            return states;
        }

    }
}