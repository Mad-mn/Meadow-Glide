using System;
using Cysharp.Threading.Tasks;
using Feature.StateModule.Scripts.Base;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.GameStateModule.Scripts.States {
    public class BootstrapState : IState {
        private readonly IViewService _viewService;
        public event Action<Type> ChangeState;

        public BootstrapState(IViewService viewService) {
            _viewService = viewService;
        }
        public void Enter() {
            Initialize().Forget();
        }

        public void Exit() {
            
        }

        private async UniTaskVoid Initialize() {
            await _viewService.Initialize();
            ChangeState?.Invoke(typeof(MainMenuState));
        }
    }
}