using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface ISceneLoadService {
    event Action<SceneType> OnSceneLoadingStarted;
    event Action<SceneType> OnSceneLoaded;
    
    SceneType CurrentScene { get; }
    
    UniTask LoadSceneAsync(SceneType sceneType, bool activateOnLoad = true, IProgress<float> progress = null, CancellationToken cancellationToken = default);
}