using System;
using UnityEngine;

namespace Feature.UIServiceModule.Scripts {
    public class UIRoot : MonoBehaviour{
        [field: SerializeField] public RectTransform CanvasRoot { get; private set; }

        private void Awake() {
            DontDestroyOnLoad(gameObject);
        }
    }
}