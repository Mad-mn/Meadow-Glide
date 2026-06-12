using System;
using System.Collections;
using Feature.CircleModule.Scripts;
using Feature.InputModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.TutorialModule.Scripts.Hints;
using UnityEngine;
using Zenject;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowPointerOnCircleState : ITutorialState {
        private const int STRIP_INDEX_FOR_HINT = 1;
        private const int START_SEGMENT_INDEX = 3;
        private const int FINISH_SEGMENT_INDEX = 1;
        private const float MOVE_DURATION = 1.5f;
        private const float DELAY_FOR_LOOP = 0.5f;

        private readonly StripModel _stripModel;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly DiContainer _container;
        private readonly IInputService _inputService;
        private readonly MoveTrackModel _moveTrackModel;

        public event Action OnComplete;

        private FingerHint _fingerHint;
        private float _stripY;
        private float _startX;
        private float _endX;
        private bool _isTapped;

        public ShowPointerOnCircleState(StripModel stripModel, ITutorialAssetProvider tutorialAssetProvider, DiContainer container,
            IInputService inputService, MoveTrackModel moveTrackModel) {
            _stripModel = stripModel;
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
            if (_stripModel.Strips.Count <= STRIP_INDEX_FOR_HINT) {
                Debug.LogError("[ShowPointerOnCircleState] Not enough strips to cache positions!");
                return;
            }

            StripController strip = _stripModel.Strips[STRIP_INDEX_FOR_HINT];
            _stripY = strip.CenterY;

            if (strip.SpawnedSegments.Count <= START_SEGMENT_INDEX) {
                Debug.LogError("[ShowPointerOnCircleState] Not enough segments on strip!");
                return;
            }

            float segmentSpan = strip.GetSegmentSpan();
            _startX = (START_SEGMENT_INDEX + 0.5f) * segmentSpan - strip.StripLoopLength * 0.5f;
            _endX = (FINISH_SEGMENT_INDEX + 0.5f) * segmentSpan - strip.StripLoopLength * 0.5f;
        }

        private void InstantiateHint() {
            FingerHint hintPrefab = _tutorialAssetProvider.GetAsset<FingerHint>(TutorialAssetType.FingerHint);
            _fingerHint = _container.InstantiatePrefab(hintPrefab).GetComponent<FingerHint>();
            _fingerHint.Enable();
        }

        private void StartHintAnimation() {
            if (_fingerHint != null)
                _fingerHint.StartCoroutine(HintRoutine());
        }

        private IEnumerator HintRoutine() {
            while (!_isTapped) {
                float elapsed = 0f;

                while (elapsed < MOVE_DURATION && !_isTapped) {
                    elapsed += Time.deltaTime;
                    float t = elapsed / MOVE_DURATION;
                    float smoothedT = Mathf.SmoothStep(0f, 1f, t);
                    float currentX = Mathf.Lerp(_startX, _endX, smoothedT);
                    UpdateHintPosition(currentX);
                    yield return null;
                }

                if (!_isTapped)
                    yield return new WaitForSeconds(DELAY_FOR_LOOP);
            }
        }

        private void UpdateHintPosition(float x) {
            if (_fingerHint == null) return;
            _fingerHint.transform.position = new Vector3(x, _stripY, 0f);
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
