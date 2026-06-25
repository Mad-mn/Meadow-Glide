using System.Collections.Generic;
using Feature.TutorialModule.Scripts.Tutorials;
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
            }

            return null;
        }

        private List<ITutorialState> GetStatesForFirstTutorial() {
            List<ITutorialState> states = new List<ITutorialState>();
            states.Add(_container.Instantiate<ShowPointerOnCircleState>());
            states.Add(_container.Instantiate<ShowPointerOnSlideAreaState>());
            return states;
        }

        private List<ITutorialState> GetStatesForUndoMoveTutorial() {
            List<ITutorialState> states = new List<ITutorialState>();
            states.Add(_container.Instantiate<ShowUndoMoveTutorialState>());
            return states;
        }

    }
}