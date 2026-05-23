using System;
using Feature.StateModule.Scripts.Base;

namespace Feature.GameStateModule.Scripts.States {
    public class GameSimpleState : IState {
        private readonly ISceneLoadService _sceneLoadService;
        public event Action<Type> ChangeState;

        public GameSimpleState(ISceneLoadService sceneLoadService) {
            _sceneLoadService = sceneLoadService;
        }

        public void Enter() {
            _sceneLoadService.LoadSceneAsync(SceneType.GameSimple);
        }

        public void Exit() { }
    }
}