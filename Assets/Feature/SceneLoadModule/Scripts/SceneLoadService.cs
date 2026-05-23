using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadService : ISceneLoadService {
    public event Action<SceneType> OnSceneLoadingStarted;
    public event Action<SceneType> OnSceneLoaded;

    public SceneType CurrentScene { get; private set; }

    public async UniTask LoadSceneAsync(SceneType sceneType, bool activateOnLoad = true, IProgress<float> progress = null, CancellationToken cancellationToken = default) {
        OnSceneLoadingStarted?.Invoke(sceneType);

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneType.ToString());
        if (loadOperation == null) {
            Debug.LogError($"[SceneLoadService] Failed to load scene: {sceneType}");
            return;
        }

        loadOperation.allowSceneActivation = activateOnLoad;

        try {
            while (!loadOperation.isDone) {
                if (cancellationToken.IsCancellationRequested) {
                    // Note: We can't actually cancel SceneManager.LoadSceneAsync once it started, 
                    // but we can stop awaiting and reporting.
                    return;
                }

                // progress goes from 0 to 0.9 before activation
                float normalizedProgress = Mathf.Clamp01(loadOperation.progress / 0.9f);
                progress?.Report(normalizedProgress);

                if (!activateOnLoad && loadOperation.progress >= 0.9f) {
                    break;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }
        }
        catch (OperationCanceledException) {
            return;
        }

        if (activateOnLoad) {
            CurrentScene = sceneType;
            OnSceneLoaded?.Invoke(sceneType);
        }
    }
}