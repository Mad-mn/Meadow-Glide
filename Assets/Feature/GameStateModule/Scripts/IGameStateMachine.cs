using Feature.StateModule.Scripts.Base;

namespace Feature.GameStateModule.Scripts {
    public interface IGameStateMachine {
        void EnterState<T>() where T : IState;
    }
}