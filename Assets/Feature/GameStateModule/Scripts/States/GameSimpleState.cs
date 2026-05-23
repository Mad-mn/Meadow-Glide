using System;
using Feature.StateModule.Scripts.Base;

namespace Feature.GameStateModule.Scripts.States {
    public class GameSimpleState : IState {
        public event Action<Type> ChangeState;

        public void Enter() {
            
        }

        public void Exit() {
        }
    }
}