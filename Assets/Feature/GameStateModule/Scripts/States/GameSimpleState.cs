using System;
using Cysharp.Threading.Tasks;
using Feature.LevelInitializeModule;
using Feature.LoadingViewModule.Scripts;
using Feature.StateModule.Scripts.Base;
using Feature.UIServiceModule.Scripts;

namespace Feature.GameStateModule.Scripts.States {
    public class GameSimpleState : IState {
        private readonly ISceneLoadService _sceneLoadService;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IViewService _viewService;
        public event Action<Type> ChangeState;

        public GameSimpleState(ISceneLoadService sceneLoadService, ILevelInitializeService levelInitializeService,
            IViewService viewService) {
            _sceneLoadService = sceneLoadService;
            _levelInitializeService = levelInitializeService;
            _viewService = viewService;
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
            _viewService.ShowView<LoadingView>(ViewType.LoadingView);
            _sceneLoadService.OnSceneLoaded -= OnLoadGameScene;
            _levelInitializeService.Dispose().Forget();
        }

        private async UniTaskVoid GameInitFlow() {
            await _levelInitializeService.Initialize();
            _viewService.HideView(ViewType.LoadingView);
        }
    }
}