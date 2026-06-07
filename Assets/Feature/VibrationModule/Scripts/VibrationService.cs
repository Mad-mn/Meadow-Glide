using System;
using DT.Haptics;
using UnityEditor;

public class VibrationService : IVibrationService {
    public void EnableVibration() {
        Haptics.CanUseHaptics = true;        
    }

    public void DisableVibration() {
        Haptics.CanUseHaptics = false;        
    }

    public void PlayVibration(VibrationType vibrationType) {
        switch (vibrationType) {
            case VibrationType.None:
                break;
            case VibrationType.Low:
                Haptics.PlayLowHaptics();
                break;
            case VibrationType.Medium:
                Haptics.PlayMidHaptics();
                break;
            case VibrationType.High:
                Haptics.PlayHeavyHaptics();
                break;
            case VibrationType.VeryHigh:
                Haptics.PlaySuperHeavyHaptics();
                break;
        }
    }
}