#if UNITY_EDITOR
using Feature.AdRecordingModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.AdRecordingModule.Scripts.Installers {
    public class AdRecordingModuleInstaller : Installer<AdRecordingModuleInstaller> {
        public override void InstallBindings() {
            var config = AdRecordingConfig.Load();
            if (config == null || !config.Enabled || config.HandCursorPrefab == null) return;

            var go = Object.Instantiate(config.HandCursorPrefab);
            go.name = "AdRecording_HandCursor";
            go.GetComponent<HandCursorFollower>().Initialize(config);
            Object.DontDestroyOnLoad(go);
        }
    }
}
#endif
