using System;
using System.Collections.Generic;

namespace Feature.TutorialModule.Scripts.Tutorials {
    public class Tutorial : ITutorial {
        private readonly List<ITutorialState> _states;
        public event Action OnComplete;

        private int _currentStateIndex;
        private ITutorialState _currentState;
        
        public Tutorial(List<ITutorialState> states) {
            _states = states;
        }

        public void Activate() {
            ActivateNext();
        }

        private void ActivateNext() {
            if(CheckForCompleteAll()) {
                OnComplete?.Invoke();
                return;
            }
            
            _currentState?.Exit();
            _currentState = _states[_currentStateIndex];
            _currentState.OnComplete += HandleStateComplete;
            _currentState?.Enter();
        }

        private void HandleStateComplete() {
            _currentState.OnComplete -= HandleStateComplete;
            _currentStateIndex++;
            ActivateNext();
        }

        private bool CheckForCompleteAll() {
            return _currentStateIndex >= _states.Count;
        }
    }
}