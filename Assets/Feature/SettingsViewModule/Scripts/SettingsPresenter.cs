using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SoundModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using AudioType = Feature.SoundModule.Scripts.AudioType;

namespace Feature.SettingsViewModule.Scripts {
    public class SettingsPresenter : PresenterBase<SettingsView> {
        private readonly SaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly IViewService _viewService;
        private readonly IAudioService _audioService;
        private readonly IVibrationService _vibrationService;

        public SettingsPresenter(SettingsView view, SaveDataModel saveDataModel, ISaveDataService saveDataService,
            IViewService viewService, IAudioService audioService, IVibrationService vibrationService): base(view) {
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _viewService = viewService;
            _audioService = audioService;
            _vibrationService = vibrationService;
        }

        public override void Initialize() {
            View.CloseButton.onClick.AddListener(CloseSettings);
            View.SoundsToggle.onValueChanged.AddListener(SoundsToggle);
            View.VibrationToggle.onValueChanged.AddListener(VibrationToggle);
        }

        public override void Show() {
            PlayerSettingsData playerSettingsData = _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings);
            View.SoundsToggle.isOn = playerSettingsData.SoundsEnabled;
            View.VibrationToggle.isOn = playerSettingsData.VibrationEnabled;
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