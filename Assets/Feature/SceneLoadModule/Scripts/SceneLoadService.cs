using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadService : ISceneLoadService {
    public event Action OnSceneLoadedAsync;

    public async UniTaskVoid LoadSceneAsync(SceneType sceneType) {
        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(sceneType.ToString());
        await loadSceneOperation.ToUniTask();
        OnSceneLoadedAsync?.Invoke();
    }
}