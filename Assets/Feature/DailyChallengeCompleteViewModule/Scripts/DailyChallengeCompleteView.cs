using Feature.DailyChallengeStartViewModule.Scripts;
using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.DailyChallengeCompleteViewModule.Scripts {
    public class DailyChallengeCompleteView : ViewBase {
        [SerializeField] private TMP_Text _movesCountText;
        [SerializeField] private ChallengeProgressBar _progressBar;
        [SerializeField] private ChallengeMilestone[] _milestones;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _restartButton;

        public TMP_Text MovesCountText => _movesCountText;
        public ChallengeProgressBar ProgressBar => _progressBar;
        public ChallengeMilestone[] Milestones => _milestones;
        public Button MainMenuButton => _mainMenuButton;
        public Button RestartButton => _restartButton;
    }
}