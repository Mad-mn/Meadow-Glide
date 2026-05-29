using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Feature.UIServiceModule.Scripts {
    public class UIRoot : MonoBehaviour{
        [SerializeField] private Canvas _canvas;
        [field: SerializeField] public RectTransform CanvasRoot { get; private set; }

        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }

        public void SetupCamera(Camera cam) {
            _canvas.worldCamera = cam;
        }
    }
}