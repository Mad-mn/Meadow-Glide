#if UNITY_EDITOR
using UnityEngine;

namespace Feature.AdRecordingModule.Scripts {
    [CreateAssetMenu(fileName = "AdRecordingConfig", menuName = "Configs/Ad Recording Config")]
    public class AdRecordingConfig : ScriptableObject {
        [SerializeField] private bool _enabled;
        [SerializeField] private GameObject _handCursorPrefab;
        [SerializeField] private Sprite _releasedSprite;
        [SerializeField] private Sprite _pressedSprite;

        public bool Enabled => _enabled;
        public GameObject HandCursorPrefab => _handCursorPrefab;
        public Sprite ReleasedSprite => _releasedSprite;
        public Sprite PressedSprite => _pressedSprite;

        private const string ResourcePath = "AdRecordingConfig";

        public static AdRecordingConfig Load() {
            return Resources.Load<AdRecordingConfig>(ResourcePath);
        }
    }
}
#endif
