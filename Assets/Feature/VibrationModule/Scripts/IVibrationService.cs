using UnityEngine;

public interface IVibrationService {
    void Initialize();
    void PlayVibration(VibrationType vibrationType);
    void ChangeEnabledState(bool enabled);
}