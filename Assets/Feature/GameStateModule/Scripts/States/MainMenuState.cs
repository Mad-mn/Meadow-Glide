using System;
using Cysharp.Threading.Tasks;
using Feature.LoadingViewModule.Scripts;
using Feature.MainMenuViewModule.Scripts;
using Feature.StateModule.Scripts.Base;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;

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
            _viewService.ReleasePrewarmedView(ViewType.WinLevel);
        }

        public void Exit() {
            _viewService.ShowView<LoadingView>(ViewType.LoadingView);
            _viewService.HideView(ViewType.MainMenu);
            _viewService.PrewarmView<WinLevel>(ViewType.WinLevel);
        }

        private async UniTaskVoid LoadMainMenuScene() {
            await _sceneLoadService.LoadSceneAsync(SceneType.MainMenu);
            _viewService.ShowView<MainMenuView>(ViewType.MainMenu);
            _viewService.HideView(ViewType.LoadingView);
        }
    }
}