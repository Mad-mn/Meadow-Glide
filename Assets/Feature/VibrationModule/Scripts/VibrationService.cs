using TechJuego.HapticFeedback;

public class VibrationService : IVibrationService {
    public void EnableVibration() {
    }

    public void DisableVibration() {
    }

    public void PlayVibration(VibrationType vibrationType) {
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
}