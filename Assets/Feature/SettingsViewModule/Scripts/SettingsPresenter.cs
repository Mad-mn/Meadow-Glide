using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SoundModule.Scripts;
using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using AudioType = Feature.SoundModule.Scripts.AudioType;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.SettingsViewModule.Scripts {
    public class SettingsPresenter : PresenterBase<SettingsView> {
        private readonly SaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IViewService _viewService;
        private readonly IAudioService _audioService;
        private readonly IVibrationService _vibrationService;
        private readonly ILocalizationService _localizationService;

        public SettingsPresenter(SettingsView view, SaveDataModel saveDataModel, ISaveDataService saveDataService,
            IViewService viewService, IAudioService audioService, IVibrationService vibrationService,
            ILocalizationService localizationService): base(view) {
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _viewService = viewService;
            _audioService = audioService;
            _vibrationService = vibrationService;
            _localizationService = localizationService;
        }

        public override void Initialize() {
            View.CloseButton.onClick.AddListener(CloseSettings);
            View.SoundsToggle.onValueChanged.AddListener(SoundsToggle);
            View.VibrationToggle.onValueChanged.AddListener(VibrationToggle);
            View.Language.onValueChanged.AddListener(OnLanguageChanged);

            PopulateLanguageDropdown();
        }

        public override void Show() {
            PlayerSettingsData playerSettingsData = _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings);
            View.SoundsToggle.isOn = playerSettingsData.SoundsEnabled;
            View.VibrationToggle.isOn = playerSettingsData.VibrationEnabled;
            View.Language.SetValueWithoutNotify((int)playerSettingsData.SelectedLanguage);
        }

        private void PopulateLanguageDropdown() {
            View.Language.ClearOptions();

            var options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
            foreach (Language lang in System.Enum.GetValues(typeof(Language))) {
                options.Add(new TMP_Dropdown.OptionData(lang.ToString()));
            }

            View.Language.AddOptions(options);
        }

        private void OnLanguageChanged(int index) {
            Language language = (Language)index;
            _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings).SelectedLanguage = language;
            SaveSettings();
            _localizationService.SetLanguage(language);
            _audioService.PlaySound(AudioType.ButtonClick);
        }

        private void VibrationToggle(bool enabled) {
            _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings)
                .VibrationEnabled = enabled;
            SaveSettings();
            _vibrationService.ChangeEnabledState(enabled);
            _audioService.PlaySound(AudioType.ButtonClick);
        }

        private void SoundsToggle(bool enabled) {
            _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings)
                .SoundsEnabled = enabled;
            SaveSettings();
            _audioService.ChangeEnabledState(enabled);
            _audioService.PlaySound(AudioType.ButtonClick);
        }

        private void SaveSettings() {
            _saveDataService.Save(SaveDataType.Settings);
        }

        private void CloseSettings() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.HideView(ViewType.SettingsView);
        }
    }
}