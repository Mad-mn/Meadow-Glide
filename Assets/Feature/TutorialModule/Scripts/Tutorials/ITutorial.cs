using System;
using Feature.CircleModule.Scripts;

namespace Feature.TutorialModule.Scripts.Tutorials {
    public interface ITutorial {
        event Action OnComplete;
        void Activate();
    }
}