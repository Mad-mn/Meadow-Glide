using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Feature.InputModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.ToolButtonViewModule.Scripts;
using Feature.ToolModule.Scripts;
using Feature.TutorialViewModule.Scripts;
using Feature.UIServiceModule.Scripts;
using DG.Tweening;
using UnityEngine;

namespace Feature.TutorialModule.Scripts.Tutorials.TutorialStates.UndoMoveTutorialStates {
    public class ShowUndoMoveTutorialState : ITutorialState {
        private const float SHAKE_DURATION = 2f;
        private const float EXPLODE_DURATION = 0.3f;
        private const float SHAKE_MAX_STRENGTH = 8f;

        private readonly IViewService _viewService;
        private readonly ToolButtonsViewModel _toolButtonsViewModel;
        private readonly IInputService _inputService;
        private readonly TutorialViewModel _tutorialViewModel;

        public event Action OnComplete;

        private ToolButton _undoButton;
        private Transform _originalButtonParent;
        private int _originalSiblingIndex;
        private Vector3 _originalButtonPosition;
        private bool _isTapped;
        private Sequence _lockAnimation;
        private CancellationTokenSource _cts;

        public ShowUndoMoveTutorialState(
            IViewService viewService,
            ToolButtonsViewModel toolButtonsViewModel,
            IInputService inputService,
            TutorialViewModel tutorialViewModel) {
            _viewService = viewService;
            _toolButtonsViewModel = toolButtonsViewModel;
            _inputService = inputService;
            _tutorialViewModel = tutorialViewModel;
        }

        public void Enter() {
            _isTapped = false;
            _cts = new CancellationTokenSource();
            _tutorialViewModel.RequestText(LocalizationKey.Tutorial_UndoMove);
            _viewService.ShowView<TutorialView>(ViewType.TutorialView);
            WaitForReadyAndRaise().Forget();
            _inputService.PointerDown += HandlePointerDown;
        }

        private async UniTaskVoid WaitForReadyAndRaise() {
            while (!_cts.Token.IsCancellationRequested) {
                ToolButton button = FindUndoButton();
                bool viewReady = _viewService.IsViewOpen(ViewType.TutorialView);

                if (button != null && viewReady) {
                    _undoButton = button;
                    RaiseUndoButton();
                    AnimateLockIcon();
                    return;
                }
                await UniTask.NextFrame(_cts.Token);
            }
        }

        private void RaiseUndoButton() {
            UIRoot uiRoot = FindUIRoot();
            if (uiRoot == null)
                return;

            Transform canvasRoot = uiRoot.CanvasRoot;

            _originalButtonParent = _undoButton.transform.parent;
            _originalSiblingIndex = _undoButton.transform.GetSiblingIndex();
            _originalButtonPosition = _undoButton.transform.position;

            _undoButton.transform.SetParent(canvasRoot);
            _undoButton.transform.position = _originalButtonPosition;
            _undoButton.transform.SetAsLastSibling();
        }

        private ToolButton FindUndoButton() {
            if (_toolButtonsViewModel.ToolButtons == null)
                return null;

            foreach (ToolButton button in _toolButtonsViewModel.ToolButtons) {
                if (button.ToolType == ToolType.Undo)
                    return button;
            }

            return null;
        }

        private UIRoot FindUIRoot() {
            UIRoot[] roots = UnityEngine.Object.FindObjectsByType<UIRoot>(FindObjectsSortMode.None);
            return roots.Length > 0 ? roots[0] : null;
        }

        private void AnimateLockIcon() {
            if (_undoButton == null || _undoButton.LockIcon == null)
                return;

            _undoButton.LockIcon.SetActive(true);

            CanvasGroup canvasGroup = _undoButton.LockIcon.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = _undoButton.LockIcon.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;

            Transform iconTransform = _undoButton.LockIcon.transform;
            iconTransform.localScale = Vector3.one;

            _lockAnimation = DOTween.Sequence();
            _lockAnimation.Append(iconTransform.DOShakePosition(SHAKE_DURATION, new Vector3(SHAKE_MAX_STRENGTH, SHAKE_MAX_STRENGTH, 0), 20, 90, false, true));
            _lockAnimation.Join(iconTransform.DOShakeScale(SHAKE_DURATION, new Vector3(0.15f, 0.15f, 0), 10, 90, false));
            _lockAnimation.AppendCallback(() => {
                iconTransform.DOScale(Vector3.one * 1.8f, EXPLODE_DURATION).SetEase(Ease.OutQuad);
                canvasGroup.DOFade(0f, EXPLODE_DURATION).SetEase(Ease.OutQuad);
            });
        }

        private void HandlePointerDown() {
            if (_isTapped)
                return;

            _isTapped = true;
            _inputService.PointerDown -= HandlePointerDown;
            OnComplete?.Invoke();
        }

        public void Exit() {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _lockAnimation?.Kill();
            _inputService.PointerDown -= HandlePointerDown;
            _viewService.HideView(ViewType.TutorialView);
            RestoreUndoButton();
        }

        private void RestoreUndoButton() {
            if (_undoButton == null)
                return;

            if (_originalButtonParent != null) {
                _undoButton.transform.SetParent(_originalButtonParent);
                _undoButton.transform.SetSiblingIndex(_originalSiblingIndex);
                _undoButton.transform.position = _originalButtonPosition;
            }

            if (_undoButton.LockIcon != null) {
                _undoButton.LockIcon.SetActive(false);
                _undoButton.LockIcon.transform.localScale = Vector3.one;
                CanvasGroup canvasGroup = _undoButton.LockIcon.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                    canvasGroup.alpha = 1f;
            }
        }
    }
}
