using System.Collections;
using Feature.CoroutineRunnerModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LevelInitializeModule;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Cysharp.Threading.Tasks;
using Feature.InputModule.Scripts;
using UnityEngine;

namespace Feature.WinLevelModule.Scripts {
    public class WinLevelPresenter : PresenterBase<WinLevel> {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IInteractionStateService _interactionStateService;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ICoroutineRunner _coroutineRunner;

        private IEnumerator _showRoutine;

        public WinLevelPresenter(WinLevel view, IGameStateMachine gameStateMachine, ILevelInitializeService levelInitializeService,
            IInteractionStateService interactionStateService, MoveTrackModel moveTrackModel, ICoroutineRunner coroutineRunner) : base(view) {
            _gameStateMachine = gameStateMachine;
            _levelInitializeService = levelInitializeService;
            _interactionStateService = interactionStateService;
            _moveTrackModel = moveTrackModel;
            _coroutineRunner = coroutineRunner;
        }

        public override void Initialize() {
            View.NextButton.onClick.AddListener(OnNextButtonClick);
            View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        }

        public override void Show() {
            base.Show();
            _interactionStateService.BlockInput();
            StartShowRoutine();
        }

        public override void Hide() {
            base.Hide();
            StopShowRoutine();
            _interactionStateService.UnblockInput();
        }

        public override void Dispose() {
            StopShowRoutine();
            base.Dispose();
        }

        private void StartShowRoutine() {
            StopShowRoutine();
            _showRoutine = ShowRoutine();
            _coroutineRunner.StartRoutine(_showRoutine);
        }

        private void StopShowRoutine() {
            if (_showRoutine != null) {
                _coroutineRunner.Stop(_showRoutine);
                _showRoutine = null;
            }
        }

        private IEnumerator ShowRoutine() {
            int maxMoves = _moveTrackModel.MaxMovesForCurrentLevel;
            int movesUsed = maxMoves - _moveTrackModel.MovesLeft;
            yield return AnimateMovesCounter(movesUsed);
        }

        private IEnumerator AnimateMovesCounter(int target) {
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                int current = Mathf.RoundToInt(Mathf.Lerp(0, target, t));
                View.MovesCount.text = current.ToString();
                yield return null;
            }

            View.MovesCount.text = target.ToString();
        }

        private void OnMainMenuButtonClick() {
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }

        private void OnNextButtonClick() {
            _levelInitializeService.ReloadScene().Forget();
        }
    }
}