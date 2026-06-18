using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using Feature.AnimationModule.Scripts;
using Feature.ChallengeModule.Scripts;
using Feature.CoroutineRunnerModule.Scripts;
using Feature.DailyChallengeStartViewModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.InputModule.Scripts;
using Feature.LevelInitializeModule;
using Feature.PlayerInventoryModule.Scripts;
using Feature.StarModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.DailyChallengeCompleteViewModule.Scripts {
    public class DailyChallengeCompletePresenter : PresenterBase<DailyChallengeCompleteView> {
        private readonly IChallengeService _challengeService;
        private readonly IChallengeConfigProvider _configProvider;
        private readonly IResourceInfoProvider _resourceInfoProvider;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IInteractionStateService _interactionStateService;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IAnimationService _animationService;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IViewService _viewService;

        private IEnumerator _showRoutine;

        public DailyChallengeCompletePresenter(
            DailyChallengeCompleteView view,
            IChallengeService challengeService,
            IChallengeConfigProvider configProvider,
            IResourceInfoProvider resourceInfoProvider,
            IGameStateMachine gameStateMachine,
            IInteractionStateService interactionStateService,
            ICoroutineRunner coroutineRunner,
            IAnimationService animationService,
            MoveTrackModel moveTrackModel,
            ILevelInitializeService levelInitializeService,
            IViewService viewService) : base(view) {
            _challengeService = challengeService;
            _configProvider = configProvider;
            _resourceInfoProvider = resourceInfoProvider;
            _gameStateMachine = gameStateMachine;
            _interactionStateService = interactionStateService;
            _coroutineRunner = coroutineRunner;
            _animationService = animationService;
            _moveTrackModel = moveTrackModel;
            _levelInitializeService = levelInitializeService;
            _viewService = viewService;
        }

        public override void Initialize() {
            View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
            View.RestartButton.onClick.AddListener(OnRestartButtonClick);
        }

        public override void Show() {
            base.Show();
            _interactionStateService.BlockInput();
            View.RestartButton.gameObject.SetActive(false);
            SetupMilestones();
            StartShowRoutine();
        }

        public override void Hide() {
            base.Hide();
            _interactionStateService.UnblockInput();
            StopShowRoutine();
        }

        public override void Dispose() {
            StopShowRoutine();
            base.Dispose();
        }

        private void SetupMilestones() {
            ChallengeConfig config = _configProvider.GetConfig(ChallengeType.Daily);
            if (config == null || config.StarRewards == null)
                return;

            for (int i = 0; i < View.Milestones.Length; i++) {
                if (i < config.StarRewards.Length) {
                    View.Milestones[i].gameObject.SetActive(true);
                    View.Milestones[i].Setup(config.StarRewards[i], _resourceInfoProvider, false);
                }
                else {
                    View.Milestones[i].gameObject.SetActive(false);
                }
            }
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

            int stars = _challengeService.GetTodayStars();
            yield return AnimateProgressBar(stars);

            yield return AnimateRewardIcons(stars);

            if (stars >= (int)StarRating.Three) {
                View.RestartButton.gameObject.SetActive(false);
            }
            else {
                View.RestartButton.gameObject.SetActive(true);
            }

            _interactionStateService.UnblockInput();
        }

        private IEnumerator AnimateProgressBar(int stars) {
            float totalFill = (float)stars / (int)StarRating.Three;
            float duration = 0.6f;
            float elapsed = 0f;
            int nextMilestoneIndex = 0;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float currentFill = Mathf.Lerp(0f, totalFill, t);
                View.ProgressBar.SetFill(currentFill);

                float milestoneThreshold = (float)(nextMilestoneIndex + 1) / (int)StarRating.Three;
                if (currentFill >= milestoneThreshold && nextMilestoneIndex < stars) {
                    ShowMilestoneWithAnimation(nextMilestoneIndex);
                    nextMilestoneIndex++;
                }

                yield return null;
            }

            View.ProgressBar.SetFill(totalFill);

            while (nextMilestoneIndex < stars) {
                ShowMilestoneWithAnimation(nextMilestoneIndex);
                nextMilestoneIndex++;
            }
        }

        private void ShowMilestoneWithAnimation(int index) {
            if (index >= View.Milestones.Length)
                return;

            ChallengeMilestone milestone = View.Milestones[index];
            milestone.ShowCheckmark();
            _animationService.PlayPunchScale(milestone.transform, Vector3.one * 0.15f, 0.25f, 2, 0.5f);
        }

        private IEnumerator AnimateMovesCounter(int target) {
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                int current = Mathf.RoundToInt(Mathf.Lerp(0, target, t));
                View.MovesCountText.text = current.ToString();
                yield return null;
            }

            View.MovesCountText.text = target.ToString();
        }

        private IEnumerator AnimateRewardIcons(int stars) {
            if (stars <= 0)
                yield break;

            for (int i = 0; i < stars && i < View.Milestones.Length; i++) {
                ChallengeMilestone milestone = View.Milestones[i];
                if (!milestone.gameObject.activeSelf)
                    continue;

                _animationService.PlayPunchScale(milestone.transform, Vector3.one * 0.2f, 0.3f, 2, 0.5f);
                yield return new WaitForSeconds(0.2f);
            }
        }

        private void OnMainMenuButtonClick() {
            _viewService.HideView(ViewType.DailyChallengeCompleteView);
            _challengeService.Deactivate();
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }

        private void OnRestartButtonClick() {
            _viewService.HideView(ViewType.DailyChallengeCompleteView);
            _levelInitializeService.ReloadScene().Forget();
        }
    }
}