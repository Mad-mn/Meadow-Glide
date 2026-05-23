using System;
using Cysharp.Threading.Tasks;
using Feature.StateModule.Scripts.Base;
using UnityEngine;

namespace Feature.GameStateModule.Scripts.States {
    public class BootstrapState : IState {
        public event Action<Type> ChangeState;

        public void Enter() {
            Initialize().Forget();
        }

        public void Exit() {
            
        }

        private async UniTaskVoid Initialize() {
            await UniTask.Yield();
            ChangeState?.Invoke(typeof(MainMenuState));
        }
    }
}