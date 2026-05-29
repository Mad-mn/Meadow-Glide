using System;
using UnityEngine;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleRegistrator : MonoBehaviour {
        [SerializeField] private CircleController _circleController;
        private GameCircleModel _circleModel;

        [Inject]
        private void InjectDependencies(GameCircleModel circleModel) {
            _circleModel = circleModel;
        }

        private void Start() {
            _circleModel.RegisterCircle(_circleController);
        }

        private void OnDestroy() {
            _circleModel.UnregisterCircle(_circleController);
        }
    }
}