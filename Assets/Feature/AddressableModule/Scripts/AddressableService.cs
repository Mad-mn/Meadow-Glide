using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Service for managing Addressable assets and instances with automatic handle tracking and caching.
/// </summary>
public class AddressableService : IAddressableService
{
    private readonly Dictionary<(string, Type), AsyncOperationHandle> _loadedAssets = new();
    private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instances = new();

    /// <summary>
    /// Loads or retrieves a cached asset by its address.
    /// </summary>
    public async UniTask<T> GetAsset<T>(string assetName)
    {
        var key = (assetName, typeof(T));
        
        if (_loadedAssets.TryGetValue(key, out var existingHandle))
        {
            if (existingHandle.IsValid())
            {
                if (!existingHandle.IsDone)
                {
                    await existingHandle.ToUniTask();
                }

                if (existingHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    return (T)existingHandle.Result;
                }
            }
            _loadedAssets.Remove(key);
        }

        var handle = Addressables.LoadAssetAsync<T>(assetName);
        _loadedAssets[key] = handle;

        try
        {
            await handle.ToUniTask();
        }
        catch (Exception e)
        {
            Debug.LogError($"[AddressableService] Exception loading asset '{assetName}' of type {typeof(T)}: {e.Message}");
            _loadedAssets.Remove(key);
            return default;
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }

        Debug.LogError($"[AddressableService] Failed to load asset '{assetName}' of type {typeof(T)}. Status: {handle.Status}");
        _loadedAssets.Remove(key);
        if (handle.IsValid()) Addressables.Release(handle);
        return default;
    }

    /// <summary>
    /// Instantiates a GameObject from Addressables and tracks its handle for proper release.
    /// </summary>
    public async UniTask<GameObject> InstantiateAsync(string assetName, Transform parent = null, bool instantiateInWorldSpace = false)
    {
        var handle = Addressables.InstantiateAsync(assetName, parent, instantiateInWorldSpace);

        try
        {
            GameObject instance = await handle.ToUniTask();
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _instances[instance] = handle;
                return instance;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AddressableService] Exception instantiating asset '{assetName}': {e.Message}");
        }

        Debug.LogError($"[AddressableService] Failed to instantiate asset '{assetName}'. Status: {handle.Status}");
        if (handle.IsValid()) Addressables.Release(handle);
        return null;
    }

    /// <summary>
    /// Releases a cached asset by its address and type.
    /// </summary>
    public void ReleaseAsset(string assetName)
    {
        // Note: This releases ALL types associated with this asset name if multiple were loaded.
        // Or we could change the signature to include type. For now, let's find all matches.
        List<(string, Type)> keysToRemove = new();
        foreach (var key in _loadedAssets.Keys)
        {
            if (key.Item1 == assetName)
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            if (_loadedAssets.TryGetValue(key, out var handle))
            {
                if (handle.IsValid()) Addressables.Release(handle);
                _loadedAssets.Remove(key);
            }
        }
    }

    /// <summary>
    /// Releases an instance and its associated Addressables handle.
    /// </summary>
    public void ReleaseInstance(GameObject instance)
    {
        if (instance == null) return;

        if (_instances.TryGetValue(instance, out var handle))
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            _instances.Remove(instance);
        }
        else
        {
            // Fallback for instances not tracked by this service
            Addressables.ReleaseInstance(instance);
        }
    }

    /// <summary>
    /// Releases all tracked assets and instances.
    /// </summary>
    public void Cleanup()
    {
        foreach (var handle in _instances.Values)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
        _instances.Clear();

        foreach (var handle in _loadedAssets.Values)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
        _loadedAssets.Clear();
    }
}