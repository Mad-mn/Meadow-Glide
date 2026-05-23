using System;
using Cysharp.Threading.Tasks;
using Feature.StateModule.Scripts.Base;

namespace Feature.GameStateModule.Scripts.States {
    public class MainMenuState : IState{
        private readonly ISceneLoadService _sceneLoadService;

        public MainMenuState(ISceneLoadService sceneLoadService) {
            _sceneLoadService = sceneLoadService;
        }

        public event Action<Type> ChangeState;

        public void Enter() {
            LoadMainMenuScene().Forget();
        }

        public void Exit() {
        }

        private async UniTaskVoid LoadMainMenuScene() { 
            _sceneLoadService.LoadSceneAsync(SceneType.MainMenu);
        }
    }
}