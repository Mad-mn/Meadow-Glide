using System;
using UnityEngine;

namespace Feature.TutorialModule.Scripts.Hints {
    public class FingerHint : MonoBehaviour {
        public Transform Transform =>
            transform;

        private void Awake() {
            Disable();
        }

        public void Enable() {
            gameObject.SetActive(true);
        }

        public void Disable() {
            gameObject.SetActive(false);
        }
    }
}