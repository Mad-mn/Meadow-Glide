using System;
using System.Collections;
using Feature.ChallengeModule.Scripts;
using Feature.CoroutineRunnerModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.MoveEfficiencyModule.Scripts;
using Feature.PlayerInventoryModule.Scripts;
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
        private readonly ILocalizationService _localizationService;

        private IEnumerator _timerRoutine;

        public DailyChallengeStartPresenter(
            DailyChallengeStartView view,
            IChallengeService challengeService,
            IChallengeConfigProvider configProvider,
            IResourceInfoProvider resourceInfoProvider,
            IGameStateMachine gameStateMachine,
            ICoroutineRunner coroutineRunner,
            IViewService viewService,
            ILocalizationService localizationService) : base(view) {
            _challengeService = challengeService;
            _configProvider = configProvider;
            _resourceInfoProvider = resourceInfoProvider;
            _gameStateMachine = gameStateMachine;
            _coroutineRunner = coroutineRunner;
            _viewService = viewService;
            _localizationService = localizationService;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(OnPlayButtonClick);
            View.CloseButton.onClick.AddListener(OnCloseButtonClick);
        }

        public override void Show() {
            base.Show();
            SetupLock();
            SetupMilestones();
            StartTimer();
            SetupMaxMovesText();
        }

        private void SetupMaxMovesText() {
            int moves = _challengeService.GetMinMoves();
            View.MaxMovesText.text = $"{_localizationService.Get(LocalizationKey.Global_Max)} {moves} {_localizationService.Get(LocalizationKey.Global_Moves)}";
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
            MoveEfficiencyResult currentResult = _challengeService.GetTodayBestResult();

            if (config == null || config.Rewards == null)
                return;

            for (int i = 0; i < View.Milestones.Length; i++) {
                if (i < config.Rewards.Length) {
                    View.Milestones[i].gameObject.SetActive(true);
                    View.Milestones[i].Setup(config.Rewards[i], _resourceInfoProvider, i < (int)currentResult);
                }
                else {
                    View.Milestones[i].gameObject.SetActive(false);
                }
            }
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