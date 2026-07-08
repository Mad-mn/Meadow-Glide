using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
    public class GuidedMoveState : ITutorialState {
        private const int SEGMENT_HIGHLIGHT_ORDER = 10000;
        private const int FINGER_HINT_ORDER = 20000;
        private const float MOVE_DURATION = 1.5f;
        private const float DELAY_FOR_LOOP = 0.5f;
        private const float AUTO_ROTATE_DURATION = 0.3f;

        private readonly StripModel _stripModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ITutorialAssetProvider _tutorialAssetProvider;
        private readonly DiContainer _container;
        private readonly IInputService _inputService;
        private readonly IViewService _viewService;
        private readonly TutorialViewModel _tutorialViewModel;
        private readonly IInteractionStateService _interactionState;
        private readonly ISlideSegmentService _slideSegmentService;

        private readonly int _targetStripIndex;
        private readonly int _targetSegmentIndex;
        private readonly LocalizationKey _textKey;

        public event Action OnComplete;

        private FingerHint _fingerHint;
        private float _startX;
        private float _endX;
        private float _positionY;
        private bool _moveCompleted;
        private StripController _targetStrip;
        private float _segmentSpan;
        private int _targetScrollSegments;
        private CancellationTokenSource _autoRotateCts;
        private readonly List<(StripSegment segment, int originalOrder)> _highlightedSegments = new();

        public GuidedMoveState(
            StripModel stripModel,
            MoveTrackModel moveTrackModel,
            ITutorialAssetProvider tutorialAssetProvider,
            DiContainer container,
            IInputService inputService,
            IViewService viewService,
            TutorialViewModel tutorialViewModel,
            IInteractionStateService interactionState,
            ISlideSegmentService slideSegmentService,
            int targetStripIndex,
            int targetSegmentIndex,
            LocalizationKey textKey) {
            _stripModel = stripModel;
            _moveTrackModel = moveTrackModel;
            _tutorialAssetProvider = tutorialAssetProvider;
            _container = container;
            _inputService = inputService;
            _viewService = viewService;
            _tutorialViewModel = tutorialViewModel;
            _interactionState = interactionState;
            _slideSegmentService = slideSegmentService;
            _targetStripIndex = targetStripIndex;
            _targetSegmentIndex = targetSegmentIndex;
            _textKey = textKey;
        }

        public void Enter() {
            _moveCompleted = false;
            _autoRotateCts?.Cancel();
            _autoRotateCts = new CancellationTokenSource();
            CacheTargetStrip();
            if (_targetStrip == null) return;

            CalculateTargetScroll();
            ShowText();
            HighlightTargetStrip();
            InstantiateHint();
            CachePositions();
            _interactionState.BlockInput();
            _interactionState.AllowedStripIndex = _targetStrip.PositionIndex;
            _stripModel.OnStripRotationStatusChanged += HandleStripRotationChanged;
            StartHintAnimation();
        }

        private void CacheTargetStrip() {
            int idx = Mathf.Clamp(_targetStripIndex, 0, _stripModel.Strips.Count - 1);
            _targetStrip = _stripModel.Strips[idx];
            _segmentSpan = _targetStrip.GetSegmentSpan();
        }

        private void CalculateTargetScroll() {
            int segCount = _targetStrip.SegmentCount;
            _targetScrollSegments = _targetSegmentIndex % segCount;
        }

        private void CachePositions() {
            _positionY = _targetStrip.CenterY;
            int segCount = _targetStrip.SegmentCount;
            float loopLength = _targetStrip.StripLoopLength;
            float segWidth = loopLength / segCount;
            float halfLoop = loopLength * 0.5f;

            int fromSeg = _targetSegmentIndex;
            int toSeg = (_targetSegmentIndex - _targetScrollSegments + segCount) % segCount;

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

            if (snapped == _targetScrollSegments) {
                _autoRotateCts?.Cancel();
                _moveCompleted = true;
                _slideSegmentService.UpdateSegmentsInAreas();
                _stripModel.SegmentsChanged();
                Complete();
            }
            else {
                AutoRotateToCorrect().Forget();
            }
        }

        private async UniTaskVoid AutoRotateToCorrect() {
            _autoRotateCts?.Cancel();
            _autoRotateCts = new CancellationTokenSource();
            var ct = _autoRotateCts.Token;

            await UniTask.Delay(100, cancellationToken: ct);
            float targetOffset = _targetScrollSegments * _segmentSpan;
            float currentOffset = _targetStrip.ScrollOffset;
            float elapsed = 0f;

            while (elapsed < AUTO_ROTATE_DURATION) {
                if (ct.IsCancellationRequested) return;
                elapsed += Time.deltaTime;
                float t = elapsed / AUTO_ROTATE_DURATION;
                t = 1f - Mathf.Pow(1f - t, 3f);
                float offset = Mathf.Lerp(currentOffset, targetOffset, t);
                _targetStrip.SetScrollOffset(offset, false);
                await UniTask.Yield(ct);
            }

            if (ct.IsCancellationRequested) return;
            _targetStrip.SetScrollOffset(targetOffset, false);
            _targetStrip.ClearWrapGhosts();
            _slideSegmentService.UpdateSegmentsInAreas();
            _stripModel.SegmentsChanged();
            _moveCompleted = true;
            Complete();
        }

        private void Complete() {
            Cleanup();
            OnComplete?.Invoke();
        }

        private void Cleanup() {
            _autoRotateCts?.Cancel();
            _autoRotateCts = null;
            _stripModel.OnStripRotationStatusChanged -= HandleStripRotationChanged;
            RestoreHighlightedSegments();
            _viewService.HideView(ViewType.TutorialView);
            _interactionState.AllowedStripIndex = -1;
            _interactionState.UnblockInput();
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
