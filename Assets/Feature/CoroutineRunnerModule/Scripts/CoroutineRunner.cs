using System.Collections;
using UnityEngine;

namespace Feature.CoroutineRunnerModule.Scripts {
    public class CoroutineRunner : MonoBehaviour, ICoroutineRunner {
        public void StartRoutine(IEnumerator routine) {
            StartCoroutine(routine);
        }

        public void Stop(IEnumerator routine) {
            StopCoroutine(routine);
        }

        public void StopAll() {
            StopAllCoroutines();
        }
    }
}
