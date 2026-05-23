using System;
using Cysharp.Threading.Tasks;
using Feature.MainMenuViewModule.Scripts;
using Feature.StateModule.Scripts.Base;
using Feature.UIServiceModule.Scripts;

namespace Feature.GameStateModule.Scripts.States {
    public class MainMenuState : IState{
        private readonly ISceneLoadService _sceneLoadService;
        private readonly IViewService _viewService;

        public MainMenuState(ISceneLoadService sceneLoadService, IViewService viewService) {
            _sceneLoadService = sceneLoadService;
            _viewService = viewService;
        }

        public event Action<Type> ChangeState;

        public void Enter() {
            LoadMainMenuScene().Forget();
        }

        public void Exit() {
            _viewService.HideView(ViewType.MainMenu);
        }

        private async UniTaskVoid LoadMainMenuScene() {
            _sceneLoadService.OnSceneLoadedAsync += OnLoadMainMenuScene;
            _sceneLoadService.LoadSceneAsync(SceneType.MainMenu);
        }

        private void OnLoadMainMenuScene(SceneType sceneType) {
            if(sceneType != SceneType.MainMenu)
                return;
            _sceneLoadService.OnSceneLoadedAsync -= OnLoadMainMenuScene;
            _viewService.ShowView<MainMenuView>(ViewType.MainMenu);
        }
    }
}