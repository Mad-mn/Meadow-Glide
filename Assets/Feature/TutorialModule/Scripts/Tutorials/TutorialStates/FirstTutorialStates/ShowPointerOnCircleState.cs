using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Feature.CircleModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.TutorialModule.Scripts.Hints;
using UnityEngine;
using Zenject;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowPointerOnCircleState : ITutorialState{
        private const int CIRCLE_INDEX_FOR_HINT = 1;
        private const int START_SEGMENT_INDEX = 3;
        private const int FINISH_SEGMENT_INDEX = 1;
        private const float MOVE_DURATION = 1.5f;
        private const float DELAY_FOR_LOOP = 0.5f;

        private readonly GameCircleModel _gameCircleModel;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly DiContainer _container;
        private readonly IInputService _inputService;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IMoveTrackService _moveTrackService;

        public event Action OnComplete;

        private FingerHint _fingerHint;
        private Vector3 _center;
        private float _radius;
        private float _startAngle;
        private float _endAngle;
        private bool _isTapped;

        public ShowPointerOnCircleState(GameCircleModel gameCircleModel, ITutorialAssetProvider tutorialAssetProvider, DiContainer container,
            IInputService inputService, MoveTrackModel moveTrackModel) {
            _gameCircleModel = gameCircleModel;
            _tutorialAssetProvider = tutorialAssetProvider;
            _container = container;
            _inputService = inputService;
            _moveTrackModel = moveTrackModel;
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
            _moveTrackModel.OnMovesChanged += WaiteForMovesChanged;
        }

        private void WaiteForMovesChanged() {
            _moveTrackModel.OnMovesChanged -= WaiteForMovesChanged;
            OnComplete?.Invoke();
        }

        private void CachePositions() {
            if (_gameCircleModel.Circles.Count < 2) {
                Debug.LogError("[ShowPointerOnCircleState] Not enough circles to cache positions!");
                return;
            }

            var circle = _gameCircleModel.Circles[CIRCLE_INDEX_FOR_HINT];
            _radius = circle.Radius;
            _center = circle.transform.position;

            if (circle.SpawnedSegments.Count < START_SEGMENT_INDEX) {
                Debug.LogError("[ShowPointerOnCircleState] Not enough segments on circle 2!");
                return;
            }

            var segStart = circle.SpawnedSegments[START_SEGMENT_INDEX]; // 3rd segment
            var segEnd = circle.SpawnedSegments[FINISH_SEGMENT_INDEX];   // 1st segment

            _startAngle = segStart.transform.localEulerAngles.z;
            _endAngle = segEnd.transform.localEulerAngles.z;
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
                    float currentAngle = Mathf.LerpAngle(_startAngle, _endAngle, smoothedT);
                    UpdateHintPosition(currentAngle);
                    yield return null;
                }

                if (!_isTapped) {
                    yield return new WaitForSeconds(DELAY_FOR_LOOP);
                }
            }
        }

        private void UpdateHintPosition(float angle) {
            if (_fingerHint == null) return;
            Vector3 offset = Quaternion.Euler(0, 0, angle) * Vector3.right * _radius;
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