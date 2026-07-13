using System.Collections;
using Feature.CoroutineRunnerModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LevelInitializeModule;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Cysharp.Threading.Tasks;
using Feature.InputModule.Scripts;
using UnityEngine;

namespace Feature.WinLevelModule.Scripts {
    public class WinLevelPresenter : PresenterBase<WinLevel> {
        private const float DURATION = 0.8f;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IInteractionStateService _interactionStateService;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly ISaveDataModel _saveDataModel;
        private readonly IUnlockProgressConfigProvider _unlockProgressConfigProvider;
        private readonly ILocalizationService _localizationService;

        private IEnumerator _showRoutine;

        public WinLevelPresenter(WinLevel view, IGameStateMachine gameStateMachine, ILevelInitializeService levelInitializeService,
            IInteractionStateService interactionStateService, MoveTrackModel moveTrackModel, ICoroutineRunner coroutineRunner,
            ISaveDataModel saveDataModel, IUnlockProgressConfigProvider unlockProgressConfigProvider,
            ILocalizationService localizationService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _levelInitializeService = levelInitializeService;
            _interactionStateService = interactionStateService;
            _moveTrackModel = moveTrackModel;
            _coroutineRunner = coroutineRunner;
            _saveDataModel = saveDataModel;
            _unlockProgressConfigProvider = unlockProgressConfigProvider;
            _localizationService = localizationService;
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
            _coroutineRunner.StartRoutine(SetupUnlockProgressBar());
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

        private IEnumerator SetupUnlockProgressBar() {
            PlayerProgressData progress = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            int currentLevel = progress.Level;
            int levelBeforeGame = currentLevel - 1;

            UnlockProgressData entry = _unlockProgressConfigProvider.GetEntryForLevel(levelBeforeGame);
            if (entry == null) {
                View.UnlockProgressBar.gameObject.SetActive(false);
                yield break;
            }

            View.UnlockProgressBar.gameObject.SetActive(true);
            View.UnlockProgressBar.Title.text = _localizationService.Get(entry.TitleLocalizationKey);
            View.UnlockProgressBar.Fill.fillAmount = 0f;

            float fromFill = (float)levelBeforeGame / entry.UnlockLevel;
            float toFill = (float)currentLevel / entry.UnlockLevel;

            yield return AnimateProgressBar(fromFill, toFill);
        }

        private IEnumerator AnimateProgressBar(float from, float to) {
            float elapsed = 0f;

            while (elapsed < DURATION) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / DURATION);
                View.UnlockProgressBar.Fill.fillAmount = Mathf.Lerp(from, to, t);
                yield return null;
            }

            View.UnlockProgressBar.Fill.fillAmount = to;
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