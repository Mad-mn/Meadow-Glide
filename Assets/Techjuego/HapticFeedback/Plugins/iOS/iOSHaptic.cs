using System.Runtime.InteropServices;

namespace TechJuego.HapticFeedback
{
    public class iOSHaptic
    {
#if UNITY_IOS
        [DllImport("__Internal")]
        public static extern void PerformUIImpactFeedbackStyleHeavy();

        [DllImport("__Internal")]
        public static extern void PerformUIImpactFeedbackStyleMedium();

        [DllImport("__Internal")]
        public static extern void PerformUIImpactFeedbackStyleLight();

        [DllImport("__Internal")]
        public static extern void PerformUIImpactFeedbackStyleRigid();

        [DllImport("__Internal")]
        public static extern void PerformUIImpactFeedbackStyleSoft();

        [DllImport("__Internal")]
        public static extern void PerformUINotificationFeedbackTypeSuccess();

        [DllImport("__Internal")]
        public static extern void PerformUINotificationFeedbackTypeError();

        [DllImport("__Internal")]
        public static extern void PerformUINotificationFeedbackTypeWarning();

        [DllImport("__Internal")]
        public static extern void PerformUISelectionFeedbackGenerator();

#endif

    }
}