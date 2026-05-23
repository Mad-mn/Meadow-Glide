using System;

namespace Feature.GameStateModule.Scripts {
    public interface IGameStateMachine {
        void EnterState(Type stateType);
    }
}