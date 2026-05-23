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
            EnterState<BootstrapState>();
        }

        public void EnterState<T>() where T : IState {
            if (!_states.TryGetValue(typeof(T), out IState state))
                return;
            
            if(_currentState == state) return;
            
            _currentState?.Exit();
            _currentState = state;
            _currentState.Enter();
        }
    }
}