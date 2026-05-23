using System;
using Cysharp.Threading.Tasks;

public interface ISceneLoadService {
    event Action OnSceneLoadedAsync;
    UniTaskVoid LoadSceneAsync(SceneType sceneType);
}