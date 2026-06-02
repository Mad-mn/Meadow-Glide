using System;

namespace Feature.TutorialModule.Scripts.Tutorials {
    public interface ITutorialState {
        event Action OnComplete;

        void Enter();
        void Exit();
    }
}