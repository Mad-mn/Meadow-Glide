using System;
using Cysharp.Threading.Tasks;
using Feature.LevelInitializeModule;
using Feature.StateModule.Scripts.Base;

namespace Feature.GameStateModule.Scripts.States {
    public class GameSimpleState : IState {
        private readonly ISceneLoadService _sceneLoadService;
        private readonly ILevelInitializeService _levelInitializeService;
        public event Action<Type> ChangeState;

        public GameSimpleState(ISceneLoadService sceneLoadService, ILevelInitializeService levelInitializeService) {
            _sceneLoadService = sceneLoadService;
            _levelInitializeService = levelInitializeService;
        }

        public void Enter() {
            _sceneLoadService.OnSceneLoaded += OnLoadGameScene;
            _sceneLoadService.LoadSceneAsync(SceneType.GameSimple).Forget();
        }

        private void OnLoadGameScene(SceneType sceneType) {
            if(sceneType != SceneType.GameSimple)
                return;
            GameInitFlow().Forget();
        }

        public void Exit() {
            _sceneLoadService.OnSceneLoaded -= OnLoadGameScene;
            _levelInitializeService.Dispose().Forget();
        }

        private async UniTaskVoid GameInitFlow() {
            await _levelInitializeService.Initialize();
        }
    }
}