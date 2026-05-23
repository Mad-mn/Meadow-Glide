using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IAddressableService
{
    UniTask<T> GetAsset<T>(string assetName);
    UniTask<GameObject> InstantiateAsync(string assetName, Transform parent = null, bool instantiateInWorldSpace = false);
    void ReleaseAsset(string assetName);
    void ReleaseInstance(GameObject instance);
}