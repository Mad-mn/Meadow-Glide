using System;
using System.Collections;
using Feature.CircleModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.TutorialModule.Scripts.Hints;
using UnityEngine;
using Zenject;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowPointerOnSlideAreaState : ITutorialState {
        private const int CIRCLE_INDEX_FOR_HINT_START = 0;
        private const int CIRCLE_INDEX_FOR_HINT_FINISH = 1;
        private const float MOVE_DURATION = 1.5f;

        private readonly GameCircleModel _gameCircleModel;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly DiContainer _container;
        private readonly IInputService _inputService;
        private readonly SlideAreaModel _slideAreaModel;
        private readonly ISlideAreaService _slideAreaService;
        public event Action OnComplete;

        private bool _isTapped;
        private FingerHint _fingerHint;
        private float _minRadius;
        private float _maRadius;
        private float _angle;
        private Vector3 _center;

        public ShowPointerOnSlideAreaState(GameCircleModel gameCircleModel, ITutorialAssetProvider tutorialAssetProvider, DiContainer container,
            IInputService inputService, SlideAreaModel slideAreaModel) {
            _gameCircleModel = gameCircleModel;
            _tutorialAssetProvider = tutorialAssetProvider;
            _container = container;
            _inputService = inputService;
            _slideAreaModel = slideAreaModel;
        }

        public void Enter() {
            _isTapped = false;
            InstantiateHint();
            CachePositions();
            _inputService.PointerDown += HandlePointerDown;
            StartHintAnimation();
        }

        private void HandlePointerDown() {
            _isTapped = true;
            _fingerHint.Disable();
            OnComplete?.Invoke();
        }

        private void CachePositions() {
            if (_gameCircleModel.Circles.Count < 2) {
                Debug.LogError("[ShowPointerOnCircleState] Not enough circles to cache positions!");
                return;
            }

            var circleStart = _gameCircleModel.Circles[CIRCLE_INDEX_FOR_HINT_START];
            var circleFinish = _gameCircleModel.Circles[CIRCLE_INDEX_FOR_HINT_FINISH];

            var segment = _slideAreaModel.SpawnedAreas[0];
            CircleSegment segStart = circleStart.SpawnedSegments[segment.SectorIndex]; // 3rd segment
            CircleSegment segEnd = circleFinish.SpawnedSegments[segment.SectorIndex];   // 1st segment
            
            _minRadius = segStart.Radius;
            _maRadius = segEnd.Radius;
            _angle = segStart.transform.localEulerAngles.z;
            _center = circleStart.transform.position;
        }

        private void InstantiateHint() {
            FingerHint hintPrefab = _tutorialAssetProvider.GetAsset<FingerHint>(TutorialAssetType.FingerHint);
            _fingerHint = _container.InstantiatePrefab(hintPrefab).GetComponent<FingerHint>();
            _fingerHint.Enable();
        }

        private void StartHintAnimation() {
            if (_fingerHint != null) {
                _fingerHint.StartCoroutine(HintRoutine());
            }
        }

        private IEnumerator HintRoutine() {
            while (!_isTapped) {
                float elapsed = 0;

                while (elapsed < MOVE_DURATION && !_isTapped) {
                    elapsed += Time.deltaTime;
                    float t = elapsed / MOVE_DURATION;
                    // Ease in out
                    float smoothedT = Mathf.SmoothStep(0, 1, t);
                    float currentRadius = Mathf.LerpAngle(_minRadius, _maRadius, smoothedT);
                    UpdateHintPosition(currentRadius);
                    yield return null;
                }

                if (!_isTapped) {
                    yield return new WaitForSeconds(DELAY_FOR_LOOP);
                }
            }
        }

        private const float DELAY_FOR_LOOP = 0.5f;

        private void UpdateHintPosition(float radius) {
            if (_fingerHint == null) return;
            Vector3 offset = Quaternion.Euler(0, 0, _angle) * Vector3.right * radius;
            _fingerHint.transform.position = _center + offset;
        }

        public void Exit() {
            _inputService.PointerDown -= HandlePointerDown;
            if (_fingerHint != null) {
                _fingerHint.Disable();
                UnityEngine.Object.Destroy(_fingerHint.gameObject);
            }
        }
    }
}