using UnityEngine;

public interface IVibrationService {
    void EnableVibration();
    void DisableVibration();
    void PlayVibration(VibrationType vibrationType);
}