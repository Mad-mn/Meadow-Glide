using System;

namespace Feature.StateModule.Scripts.Base {
    public interface IState {
        public event Action<Type> ChangeState;

        void Enter();
        void Exit();
    }
}