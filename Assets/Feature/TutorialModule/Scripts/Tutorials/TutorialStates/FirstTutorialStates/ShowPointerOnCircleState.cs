using System;
using Feature.CircleModule.Scripts;
using Feature.TutorialModule.Scripts.Hints;
using UnityEngine;
using Zenject;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowPointerOnCircleState : ITutorialState{
        private readonly ICircleControllerService _circleControllerService;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly DiContainer _container;
        public event Action OnComplete;

        public ShowPointerOnCircleState(ICircleControllerService circleControllerService, ITutorialAssetProvider tutorialAssetProvider, DiContainer container) {
            _circleControllerService = circleControllerService;
            _tutorialAssetProvider = tutorialAssetProvider;
            _container = container;
        }
        
        public void Enter() {
            InstantiateHint();
        }

        private void InstantiateHint() {
            FingerHint hintPrefab = _tutorialAssetProvider.GetAsset<FingerHint>(TutorialAssetType.FingerHint);
            FingerHint hint = _container.InstantiatePrefab(hintPrefab).GetComponent<FingerHint>();
        }

        public void Exit() {
            
        }
    }
}