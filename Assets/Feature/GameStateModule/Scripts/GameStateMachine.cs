using System;
using System.Collections.Generic;
using Feature.GameStateModule.Scripts.States;
using Feature.StateModule.Scripts.Base;
using UnityEngine;
using Zenject;

namespace Feature.GameStateModule.Scripts {
    public class GameStateMachine : IGameStateMachine, IInitializable{
        private readonly Dictionary<Type, IState> _states = new ();
        private IState _currentState;

        public GameStateMachine(BootstrapState bootstrapState, MainMenuState mainMenuState, GameSimpleState gameSimpleState) {
            _states.Add(typeof(BootstrapState), bootstrapState);
            _states.Add(typeof(MainMenuState), mainMenuState);
            _states.Add(typeof(GameSimpleState), gameSimpleState);
        }

        public void Initialize() {
            EnterState(typeof(BootstrapState));
        }

        public void EnterState(Type stateType){
            if (!_states.TryGetValue(stateType, out IState state))
                return;
            
            if(_currentState == state) return;
            
            _currentState?.Exit();
            _currentState = state;
            _currentState.ChangeState += OnCompleteState;

            _currentState.Enter();
        }

        private void OnCompleteState(Type nextStateType) {
            _currentState.ChangeState -= OnCompleteState;
            EnterState(nextStateType);
        }
    }
}