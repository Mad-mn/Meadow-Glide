using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.SlideAreaModule.Scripts;
using Feature.StripsModule.Scripts;
using Feature.TutorialModule.Scripts.Hints;
using Feature.TrackMoveModule.Scripts;
using Feature.TutorialViewModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class GuidedSlideState : ITutorialState {
        private const int HIGHLIGHT_SORTING_ORDER = 10000;
        private const float MOVE_DURATION = 1.5f;
        private const float DELAY_FOR_LOOP = 0.5f;

        private readonly StripModel _stripModel;
        private readonly SlideAreaModel _slideAreaModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly DiContainer _container;
        private readonly IInputService _inputService;
        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;
        private readonly LocalizationKey _textKey;

        private readonly int _targetAreaIndex;
        private readonly int _targetShift;

        public event Action OnComplete;

        private FingerHint _fingerHint;
        private float _startY;
        private float _endY;
        private float _columnX;
        private bool _moveCompleted;
        private SlideArea _targetArea;
        private readonly List<(StripSegment segment, int originalOrder)> _highlightedSegments = new();

        public GuidedSlideState(
            StripModel stripModel,
            SlideAreaModel slideAreaModel,
            MoveTrackModel moveTrackModel,
            ITutorialAssetProvider tutorialAssetProvider,
            DiContainer container,
            IInputService inputService,
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            int targetAreaIndex = 0,
            int targetShift = 1,
            LocalizationKey textKey = LocalizationKey.Tutorial_SlideArea) {
            _stripModel = stripModel;
            _slideAreaModel = slideAreaModel;
            _moveTrackModel = moveTrackModel;
            _tutorialAssetProvider = tutorialAssetProvider;
            _container = container;
            _inputService = inputService;
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _targetAreaIndex = targetAreaIndex;
            _targetShift = targetShift;
            _textKey = textKey;
        }

        public void Enter() {
            _moveCompleted = false;
            CacheTargetArea();
            if (_targetArea == null) return;

            ShowText();
            HighlightSlideAreaSegments();
            InstantiateHint();
            CachePositions();
            _moveTrackModel.OnMovesChanged += HandleMoveCompleted;
            StartHintAnimation();
        }

        private void ShowText() {
            _tutorialViewModel.RequestText(_textKey);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
        }

        private void HighlightSlideAreaSegments() {
            _highlightedSegments.Clear();
            foreach (StripController strip in _stripModel.Strips) {
                foreach (StripSegment segment in strip.SpawnedSegments) {
                    if (_slideAreaModel.SegmentsInAreas.Contains(segment)) {
                        int original = segment.GetSortingOrder();
                        _highlightedSegments.Add((segment, original));
                        segment.SetSortingOrder(HIGHLIGHT_SORTING_ORDER);
                    }
                }
            }
        }

        private void RestoreHighlightedSegments() {
            foreach (var (segment, originalOrder) in _highlightedSegments) {
                if (segment != null)
                    segment.SetSortingOrder(originalOrder);
            }
            _highlightedSegments.Clear();
        }

        private void CacheTargetArea() {
            int idx = Mathf.Clamp(_targetAreaIndex, 0, _slideAreaModel.SpawnedAreas.Count - 1);
            _targetArea = _slideAreaModel.SpawnedAreas[idx];
        }

        private void CachePositions() {
            int startIdx = Mathf.Min(_targetArea.StartCircleIndex, _stripModel.Strips.Count - 1);
            int endIdx = Mathf.Min(_targetArea.EndCircleIndex, _stripModel.Strips.Count - 1);

            StripController startStrip = _stripModel.Strips[startIdx];
            StripController endStrip = _stripModel.Strips[endIdx];

            float segmentSpan = startStrip.GetSegmentSpan();
            _columnX = (_targetArea.SectorIndex + 0.5f) * segmentSpan - startStrip.StripLoopLength * 0.5f;

            if (_targetShift > 0) {
                _startY = startStrip.CenterY + 0.3f;
                _endY = endStrip.CenterY - 0.3f;
            }
            else {
                _startY = endStrip.CenterY - 0.3f;
                _endY = startStrip.CenterY + 0.3f;
            }
        }

        private void InstantiateHint() {
            FingerHint hintPrefab = _tutorialAssetProvider.GetAsset<FingerHint>(TutorialAssetType.FingerHint);
            _fingerHint = _container.InstantiatePrefab(hintPrefab).GetComponent<FingerHint>();
            _fingerHint.Enable();
            var renderers = _fingerHint.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) {
                r.sortingOrder = 20000;
            }
            _fingerHint.transform.SetAsLastSibling();
        }

        private void StartHintAnimation() {
            if (_fingerHint != null)
                _fingerHint.StartCoroutine(HintRoutine());
        }

        private IEnumerator HintRoutine() {
            while (!_moveCompleted) {
                float elapsed = 0f;
                while (elapsed < MOVE_DURATION && !_moveCompleted) {
                    elapsed += Time.deltaTime;
                    float t = elapsed / MOVE_DURATION;
                    float smoothedT = Mathf.SmoothStep(0f, 1f, t);
                    float currentY = Mathf.Lerp(_startY, _endY, smoothedT);
                    if (_fingerHint != null)
                        _fingerHint.transform.position = new Vector3(_columnX, currentY, 0f);
                    yield return null;
                }
                if (!_moveCompleted)
                    yield return new WaitForSeconds(DELAY_FOR_LOOP);
            }
        }

        private void HandleMoveCompleted() {
            _moveCompleted = true;
            Complete();
        }

        private void Complete() {
            _moveTrackModel.OnMovesChanged -= HandleMoveCompleted;
            RestoreHighlightedSegments();
            _viewService.HideView(ViewType.TutorialView);
            if (_fingerHint != null) {
                _fingerHint.Disable();
                UnityEngine.Object.Destroy(_fingerHint.gameObject);
            }
            OnComplete?.Invoke();
        }

        public void Exit() {
            _moveTrackModel.OnMovesChanged -= HandleMoveCompleted;
            RestoreHighlightedSegments();
            _viewService.HideView(ViewType.TutorialView);
            if (_fingerHint != null) {
                _fingerHint.Disable();
                UnityEngine.Object.Destroy(_fingerHint.gameObject);
            }
        }
    }
}
