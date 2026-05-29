using System;
using Cysharp.Threading.Tasks;
using Feature.CameraServiceModule.Scripts;
using Feature.StateModule.Scripts.Base;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.GameStateModule.Scripts.States {
    public class BootstrapState : IState {
        private readonly IViewService _viewService;
        private readonly ICameraService _cameraService;
        public event Action<Type> ChangeState;

        public BootstrapState(IViewService viewService, ICameraService cameraService) {
            _viewService = viewService;
            _cameraService = cameraService;
        }
        public void Enter() {
            Initialize().Forget();
        }

        public void Exit() { }

        private async UniTaskVoid Initialize() {
            await _cameraService.Initialize();
            await _viewService.Initialize();
            ChangeState?.Invoke(typeof(MainMenuState));
        }
    }
}