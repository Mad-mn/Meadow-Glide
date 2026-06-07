using UnityEngine;
namespace TechJuego.HapticFeedback
{
    public class HapticInitialize
    {
        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeMethodLoad()
        {
            AndroidHaptic.LoadPluing();
        }
    }
    public class AndroidHaptic
    {
        static AndroidJavaClass unityclass;
        static AndroidJavaObject unityActivity;
        static AndroidJavaObject _PluginInstance;

        public static void LoadPluing()
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            ItializePlugin("com.techjuego.hapticfeedback.HapticInstance");
            #endif
        }
        private static void ItializePlugin(string pluginName)
        {
            unityclass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            unityActivity = unityclass.GetStatic<AndroidJavaObject>("currentActivity");
            _PluginInstance = new AndroidJavaObject(pluginName);
            if (_PluginInstance != null)
            {
                Debug.Log("Done");
            }
            _PluginInstance.CallStatic("receiveUnityActivity", unityActivity);
        }

        public static void PerformUIImpactFeedbackStyleHeavy()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUIImpactFeedbackStyleHeavy");
            }
        }

        public static void PerformUIImpactFeedbackStyleMedium()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUIImpactFeedbackStyleMedium");
            }
        }

        public static void PerformUIImpactFeedbackStyleLight()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUIImpactFeedbackStyleLight");
            }
        }

        public static void PerformUIImpactFeedbackStyleRigid()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUIImpactFeedbackStyleRigid");
            }
        }

        public static void PerformUIImpactFeedbackStyleSoft()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUIImpactFeedbackStyleSoft");
            }
        }

        public static void PerformUINotificationFeedbackTypeSuccess()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUINotificationFeedbackTypeSuccess");
            }
        }

        public static void PerformUINotificationFeedbackTypeError()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUINotificationFeedbackTypeError");
            }
        }
        public static void PerformUINotificationFeedbackTypeWarning()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUINotificationFeedbackTypeWarning");
            }
        }
        public static void PerformUISelectionFeedbackGenerator()
        {
            if (_PluginInstance != null)
            {
                _PluginInstance.Call("PerformUISelectionFeedbackGenerator");
            }
        }
    }
}