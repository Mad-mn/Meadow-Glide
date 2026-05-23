using System;
using Cysharp.Threading.Tasks;

public interface ISceneLoadService {
    event Action<SceneType> OnSceneLoadedAsync;
    UniTaskVoid LoadSceneAsync(SceneType sceneType);
}