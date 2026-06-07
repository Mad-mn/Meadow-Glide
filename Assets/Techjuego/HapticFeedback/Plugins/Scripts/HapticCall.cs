namespace TechJuego.HapticFeedback
{
    public class HapticCall
    {
        public static void HeavyHaptic()
        {
#if UNITY_IOS
        iOSHaptic.PerformUIImpactFeedbackStyleHeavy();
#endif
#if UNITY_ANDROID
        AndroidHaptic.PerformUIImpactFeedbackStyleHeavy();
#endif
        }
        public static void MediumHaptic()
        {
#if UNITY_IOS
            iOSHaptic.PerformUIImpactFeedbackStyleMedium();
#endif
#if UNITY_ANDROID
            AndroidHaptic.PerformUIImpactFeedbackStyleMedium();
#endif
        }
        public static void LightHaptic()
        {
#if UNITY_IOS
            iOSHaptic.PerformUIImpactFeedbackStyleLight();
#endif
#if UNITY_ANDROID
            AndroidHaptic.PerformUIImpactFeedbackStyleLight();
#endif
        }
        public static void RigidHaptic()
        {
#if UNITY_IOS
            iOSHaptic.PerformUIImpactFeedbackStyleRigid();
#endif
#if UNITY_ANDROID
            AndroidHaptic.PerformUIImpactFeedbackStyleRigid();
#endif
        }
        public static void SoftHaptic()
        {
#if UNITY_IOS
            iOSHaptic.PerformUIImpactFeedbackStyleSoft();
#endif
#if UNITY_ANDROID
            AndroidHaptic.PerformUIImpactFeedbackStyleSoft();
#endif
        }
        public static void PerformSuccessFeedback()
        {
#if UNITY_IOS
            iOSHaptic.PerformUINotificationFeedbackTypeSuccess();
#endif
#if UNITY_ANDROID
            AndroidHaptic.PerformUINotificationFeedbackTypeSuccess();
#endif
        }
        public static void PerformErrorFeedback()
        {
#if UNITY_IOS
            iOSHaptic.PerformUINotificationFeedbackTypeError();
#endif
#if UNITY_ANDROID
            AndroidHaptic.PerformUINotificationFeedbackTypeError();
#endif
        }
        public static void PerformWarningFeedback()
        {
#if UNITY_IOS
            iOSHaptic.PerformUINotificationFeedbackTypeWarning();
#endif
#if UNITY_ANDROID
            AndroidHaptic.PerformUINotificationFeedbackTypeWarning();
#endif
        }
    }
}