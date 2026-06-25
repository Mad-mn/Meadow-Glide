using System;
using System.Collections;
using Feature.InputModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TutorialModule.Scripts.Hints;
using UnityEngine;
using Zenject;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class ShowPointerOnSlideAreaState : ITutorialState {
        private const int STRIP_INDEX_FOR_HINT_START = 0;
        private const int STRIP_INDEX_FOR_HINT_FINISH = 1;
        private const float MOVE_DURATION = 1.5f;
        private const float DELAY_FOR_LOOP = 0.5f;

        private readonly StripModel _stripModel;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly DiContainer _container;
        private readonly IInputService _inputService;
        private readonly SlideAreaModel _slideAreaModel;

        public event Action OnComplete;

        private bool _isTapped;
        private FingerHint _fingerHint;
        private float _startY;
        private float _endY;
        private float _columnX;

        public ShowPointerOnSlideAreaState(StripModel stripModel, ITutorialAssetProvider tutorialAssetProvider, DiContainer container,
            IInputService inputService, SlideAreaModel slideAreaModel) {
            _stripModel = stripModel;
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
            if (_stripModel.Strips.Count < 2 || _slideAreaModel.SpawnedAreas.Count == 0) {
                Debug.LogError("[ShowPointerOnSlideAreaState] Not enough strips or slide areas!");
                return;
            }

            StripController stripStart = _stripModel.Strips[STRIP_INDEX_FOR_HINT_START];
            StripController stripEnd = _stripModel.Strips[STRIP_INDEX_FOR_HINT_FINISH];
            SlideArea area = _slideAreaModel.SpawnedAreas[0];

            _startY = stripStart.CenterY;
            _endY = stripEnd.CenterY;

            float segmentSpan = stripStart.GetSegmentSpan();
            _columnX = (area.SectorIndex + 0.5f) * segmentSpan - stripStart.StripLoopLength * 0.5f;
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
                    float currentY = Mathf.Lerp(_startY, _endY, smoothedT);
                    UpdateHintPosition(currentY);
                    yield return null;
                }

                if (!_isTapped)
                    yield return new WaitForSeconds(DELAY_FOR_LOOP);
            }
        }

        private void UpdateHintPosition(float y) {
            if (_fingerHint == null) return;
            _fingerHint.transform.position = new Vector3(_columnX, y, 0f);
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
