using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.PerfectMapViewModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LevelInfoPerfectChallenge : MonoBehaviour {
    [SerializeField] private TMP_Text _levelNumberText;
    [SerializeField] private TMP_Text _bestResult;
    [SerializeField] private TMP_Text _perfectResult;
    [SerializeField] private TMP_Text _rewardText;
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _claimButton;
    [SerializeField] private GameObject _perfectIcon;
    [SerializeField] private GameObject _claimedTxt;

    private ILocalizationService _localizationService;
    public Button PlayButton =>
        _playButton;
    public Button ClaimButton =>
        _claimButton;

    [Inject]
    private void Construct(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public void Setup(LevelInfoPerfectData data) {
        _levelNumberText.text = $"{_localizationService.Get(LocalizationKey.Global_Level)}: {data.LevelNumber.ToString()}";
        _bestResult.text = $"{_localizationService.Get(LocalizationKey.PerfectChallenge_YouResult)}: {data.BestMoves.ToString()}";
        _perfectResult.text = $"{_localizationService.Get(LocalizationKey.PerfectChallenge_Perfect)}: {data.ShortestSolution.ToString()}";

        _perfectIcon.SetActive(data.State == LevelPerfectState.PerfectClaimed  || data.State == LevelPerfectState.PerfectNotClaimed);
        _playButton.gameObject.SetActive(data.State == LevelPerfectState.NotCompleted ||
                                         data.State == LevelPerfectState.CompletedNotPerfect);

        _claimButton.gameObject.SetActive(data.State ==LevelPerfectState.PerfectNotClaimed);
        _claimedTxt.SetActive(data.State == LevelPerfectState.PerfectClaimed);
        _rewardText.gameObject.SetActive(data.State == LevelPerfectState.PerfectNotClaimed);

        if (data.State == LevelPerfectState.PerfectNotClaimed) {
            _rewardText.text = $"+{data.RewardAmount}";
        }
    }
}