using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using TechJuego.HapticFeedback;

public class VibrationService : IVibrationService {
    private readonly SaveDataModel _saveDataModel;
    private bool _canPlay;

    public VibrationService(SaveDataModel saveDataModel) {
        _saveDataModel = saveDataModel;
    }
    public void Initialize() {
        ChangeEnabledState(_saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings).VibrationEnabled);
    }

    public void PlayVibration(VibrationType vibrationType) {
        if (!_canPlay)
            return;

        switch (vibrationType) {
            case VibrationType.None:
                break;
            case VibrationType.Low:
                HapticCall.LightHaptic();
                break;
            case VibrationType.Medium:
                HapticCall.MediumHaptic();
                break;
            case VibrationType.High:
                HapticCall.HeavyHaptic();
                break;
            case VibrationType.VeryHigh:
                break;
        }
    }

    public void ChangeEnabledState(bool enabled) {
        _canPlay = enabled;
    }
}