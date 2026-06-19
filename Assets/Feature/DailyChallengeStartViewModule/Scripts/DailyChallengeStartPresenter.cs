using System;
using System.Collections;
using Feature.ChallengeModule.Scripts;
using Feature.CoroutineRunnerModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.InputModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
using Feature.StarModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.DailyChallengeStartViewModule.Scripts {
    public class DailyChallengeStartPresenter : PresenterBase<DailyChallengeStartView> {
        private readonly IChallengeService _challengeService;
        private readonly IChallengeConfigProvider _configProvider;
        private readonly IResourceInfoProvider _resourceInfoProvider;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ICoroutineRunner _coroutineRunner;
        private readonly IViewService _viewService;

        private IEnumerator _timerRoutine;

        public DailyChallengeStartPresenter(
            DailyChallengeStartView view,
            IChallengeService challengeService,
            IChallengeConfigProvider configProvider,
            IResourceInfoProvider resourceInfoProvider,
            IGameStateMachine gameStateMachine,
            ICoroutineRunner coroutineRunner,
            IViewService viewService) : base(view) {
            _challengeService = challengeService;
            _configProvider = configProvider;
            _resourceInfoProvider = resourceInfoProvider;
            _gameStateMachine = gameStateMachine;
            _coroutineRunner = coroutineRunner;
            _viewService = viewService;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(OnPlayButtonClick);
            View.CloseButton.onClick.AddListener(OnCloseButtonClick);
        }

        public override void Show() {
            base.Show();
            SetupLock();
            SetupMilestones();
            SetupProgressBar();
            StartTimer();
        }

        private void SetupLock() {
            bool locked = !_challengeService.IsDailyChallengeAvailable();
            View.LockIcon.gameObject.SetActive(locked);
            View.LockText.gameObject.SetActive(locked);
        }

        public override void Hide() {
            base.Hide();
            StopTimer();
        }

        public override void Dispose() {
            StopTimer();
            base.Dispose();
        }

        private void SetupMilestones() {
            ChallengeConfig config = _configProvider.GetConfig(ChallengeType.Daily);
            int stars = _challengeService.GetTodayStars();

            if (config == null || config.StarRewards == null)
                return;

            for (int i = 0; i < View.Milestones.Length; i++) {
                if (i < config.StarRewards.Length) {
                    View.Milestones[i].gameObject.SetActive(true);
                    View.Milestones[i].Setup(config.StarRewards[i], _resourceInfoProvider, stars >= i + 1);
                }
                else {
                    View.Milestones[i].gameObject.SetActive(false);
                }
            }
        }

        private void SetupProgressBar() {
            int stars = _challengeService.GetTodayStars();
            int maxStars = (int)StarRating.Three;
            float fill = maxStars > 0 ? (float)stars / maxStars : 0f;
            View.ProgressBar.SetFill(fill);
        }

        private void StartTimer() {
            StopTimer();
            _timerRoutine = TimerRoutine();
            _coroutineRunner.StartRoutine(_timerRoutine);
        }

        private void StopTimer() {
            if (_timerRoutine != null) {
                _coroutineRunner.Stop(_timerRoutine);
                _timerRoutine = null;
            }
        }

        private IEnumerator TimerRoutine() {
            while (true) {
                TimeSpan remaining = _challengeService.GetTimeUntilNextDay();
                View.TimerText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
                yield return new WaitForSeconds(1f);
            }
        }

        private void OnPlayButtonClick() {
            if(!_challengeService.IsDailyChallengeAvailable())
                return;
            _viewService.HideView(ViewType.DailyChallengeStartView);
            _challengeService.ActivateDailyChallenge(null);
            _gameStateMachine.EnterState(typeof(GameSimpleState));
        }

        private void OnCloseButtonClick() {
            View.Hide();
        }
    }
}