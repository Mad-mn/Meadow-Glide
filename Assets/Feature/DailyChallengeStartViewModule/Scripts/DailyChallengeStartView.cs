using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.DailyChallengeStartViewModule.Scripts {
    public class DailyChallengeStartView : ViewBase {
        [SerializeField] private ChallengeMilestone[] _milestones;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private TMP_Text _maxMovesText;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _lockIcon;
        [SerializeField] private GameObject _lockText;

        public ChallengeMilestone[] Milestones => _milestones;
        public TMP_Text TimerText => _timerText;
        public TMP_Text MaxMovesText => _maxMovesText;
        public Button PlayButton => _playButton;
        public Button CloseButton => _closeButton;
        public GameObject LockIcon => _lockIcon;
        public GameObject LockText => _lockText;
    }
}