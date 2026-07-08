using System;
using System.Collections;
using System.Collections.Generic;
using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.StripsModule.Scripts;
using Feature.TutorialModule.Scripts.Hints;
using Feature.TrackMoveModule.Scripts;
using Feature.TutorialViewModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.FirstTutorialStates {
    public class GuidedMoveState : ITutorialState {
        private const int SEGMENT_HIGHLIGHT_ORDER = 10000;
        private const int FINGER_HINT_ORDER = 20000;
        private const float MOVE_DURATION = 1.5f;
        private const float DELAY_FOR_LOOP = 0.5f;

        private readonly StripModel _stripModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly DiContainer _container;
        private readonly IInputService _inputService;
        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;

        private readonly int _targetStripIndex;
        private readonly int _targetOffsetSegments;
        private readonly LocalizationKey _textKey;

        public event Action OnComplete;

        private FingerHint _fingerHint;
        private float _startX;
        private float _endX;
        private float _positionY;
        private bool _moveCompleted;
        private StripController _targetStrip;
        private float _segmentSpan;
        private readonly List<(StripSegment segment, int originalOrder)> _highlightedSegments = new();

        public GuidedMoveState(
            StripModel stripModel,
            MoveTrackModel moveTrackModel,
            ITutorialAssetProvider tutorialAssetProvider,
            DiContainer container,
            IInputService inputService,
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            int targetStripIndex,
            int targetOffsetSegments,
            LocalizationKey textKey) {
            _stripModel = stripModel;
            _moveTrackModel = moveTrackModel;
            _tutorialAssetProvider = tutorialAssetProvider;
            _container = container;
            _inputService = inputService;
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _targetStripIndex = targetStripIndex;
            _targetOffsetSegments = targetOffsetSegments;
            _textKey = textKey;
        }

        public void Enter() {
            _moveCompleted = false;
            CacheTargetStrip();
            if (_targetStrip == null) return;

            ShowText();
            HighlightTargetStrip();
            InstantiateHint();
            CachePositions();
            _stripModel.OnStripRotationStatusChanged += HandleStripRotationChanged;
            StartHintAnimation();
        }

        private void CacheTargetStrip() {
            int idx = Mathf.Clamp(_targetStripIndex, 0, _stripModel.Strips.Count - 1);
            _targetStrip = _stripModel.Strips[idx];
            _segmentSpan = _targetStrip.GetSegmentSpan();
        }

        private void CachePositions() {
            _positionY = _targetStrip.CenterY;
            int segCount = _targetStrip.SegmentCount;
            float loopLength = _targetStrip.StripLoopLength;
            float segWidth = loopLength / segCount;
            float halfLoop = loopLength * 0.5f;

            int fromSeg = Mathf.Clamp(segCount - 1, 0, segCount - 1);
            int toSeg = Mathf.Clamp(segCount - 1 - Mathf.Abs(_targetOffsetSegments), 0, segCount - 1);

            if (_targetOffsetSegments > 0) {
                fromSeg = segCount - 1;
                toSeg = segCount - 1 - _targetOffsetSegments;
            }
            else if (_targetOffsetSegments < 0) {
                fromSeg = 0;
                toSeg = Mathf.Abs(_targetOffsetSegments);
            }

            _startX = (fromSeg + 0.5f) * segWidth - halfLoop;
            _endX = (toSeg + 0.5f) * segWidth - halfLoop;
        }

        private void ShowText() {
            _tutorialViewModel.RequestText(_textKey);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
        }

        private void HighlightTargetStrip() {
            _highlightedSegments.Clear();
            foreach (StripSegment segment in _targetStrip.SpawnedSegments) {
                int original = segment.GetSortingOrder();
                _highlightedSegments.Add((segment, original));
                segment.SetSortingOrder(SEGMENT_HIGHLIGHT_ORDER);
            }
        }

        private void RestoreHighlightedSegments() {
            foreach (var (segment, originalOrder) in _highlightedSegments) {
                if (segment != null)
                    segment.SetSortingOrder(originalOrder);
            }
            _highlightedSegments.Clear();
        }

        private void InstantiateHint() {
            FingerHint hintPrefab = _tutorialAssetProvider.GetAsset<FingerHint>(TutorialAssetType.FingerHint);
            _fingerHint = _container.InstantiatePrefab(hintPrefab).GetComponent<FingerHint>();
            _fingerHint.Enable();
            SetHintSortingOrder();
        }

        private void SetHintSortingOrder() {
            if (_fingerHint == null) return;
            var renderers = _fingerHint.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers) {
                r.sortingOrder = FINGER_HINT_ORDER;
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
                    float smoothed = Mathf.SmoothStep(0f, 1f, t);
                    float x = Mathf.Lerp(_startX, _endX, smoothed);
                    if (_fingerHint != null)
                        _fingerHint.transform.position = new Vector3(x, _positionY, -1f);
                    yield return null;
                }
                if (!_moveCompleted)
                    yield return new WaitForSeconds(DELAY_FOR_LOOP);
            }
        }

        private void HandleStripRotationChanged(StripController strip, bool isRotating) {
            if (strip != _targetStrip || isRotating) return;

            float offset = strip.ScrollOffset;
            int snapped = Mathf.RoundToInt(offset / _segmentSpan);

            if (snapped == _targetOffsetSegments) {
                _moveCompleted = true;
                Complete();
            }
        }

        private void Complete() {
            Cleanup();
            OnComplete?.Invoke();
        }

        private void Cleanup() {
            _stripModel.OnStripRotationStatusChanged -= HandleStripRotationChanged;
            RestoreHighlightedSegments();
            _viewService.HideView(ViewType.TutorialView);
            if (_fingerHint != null) {
                _fingerHint.Disable();
                UnityEngine.Object.Destroy(_fingerHint.gameObject);
            }
        }

        public void Exit() {
            Cleanup();
        }
    }
}
