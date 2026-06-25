using System;
using System.Collections;
using UnityEngine;

namespace Feature.CoroutineRunnerModule.Scripts {
    public interface ICoroutineRunner {
        void StartRoutine(IEnumerator routine);
        void Stop(IEnumerator routine);
        void StopAll();
    }
}
