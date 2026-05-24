using Cysharp.Threading.Tasks;
using Zenject;
using UnityEngine;

public static class ZenjectAddressablesExtensions
{
    /// <summary>
    /// Біндить асет з Addressables. 
    /// Увага: Отримання T напряму заблокує основний потік, якщо асет ще не завантажений.
    /// Рекомендується ін'єктувати UniTask&lt;T&gt; для асинхронного доступу.
    /// </summary>
    public static void BindAddressableAsset<T>(this DiContainer container, string addressKey)
    {
        container.Bind<UniTask<T>>()
            .FromMethod(ctx => ctx.Container.Resolve<IAddressableService>().GetAsset<T>(addressKey).Preserve())
            .AsSingle();

        container.Bind<T>()
            .FromMethod(ctx => ctx.Container.Resolve<IAddressableService>().GetAsset<T>(addressKey).GetAwaiter().GetResult())
            .AsSingle();
    }

    /// <summary>
    /// Біндить компонент з префабу Addressables (черех Instantiate). 
    /// Увага: Отримання T напряму заблокує основний потік для інстанціювання.
    /// Рекомендується ін'єктувати UniTask&lt;T&gt; для асинхронного доступу.
    /// </summary>
    public static void BindAddressableComponent<T>(this DiContainer container, string addressKey) where T : Component
    {
        container.Bind<UniTask<T>>()
            .FromMethod(ctx =>
            {
                async UniTask<T> GetTask()
                {
                    var service = ctx.Container.Resolve<IAddressableService>();
                    var instance = await service.InstantiateAsync(addressKey);
                    return instance != null ? instance.GetComponent<T>() : null;
                }
                return GetTask().Preserve();
            })
            .AsSingle();

        container.Bind<T>()
            .FromMethod(ctx =>
            {
                var service = ctx.Container.Resolve<IAddressableService>();
                var instance = service.InstantiateAsync(addressKey).GetAwaiter().GetResult();
                return instance != null ? instance.GetComponent<T>() : null;
            })
            .AsSingle();
    }

    /// <summary>
    /// Біндить компонент з префабу Addressables як префаб для подальшого інстанціювання (не створює екземпляр відразу).
    /// Використовувати, коли потрібно ін'єктувати T як префаб для DiContainer.InstantiatePrefabForComponent.
    /// </summary>
    public static void BindAddressablePrefabComponent<T>(this DiContainer container, string addressKey) where T : Component
    {
        container.Bind<UniTask<T>>()
            .FromMethod(ctx =>
            {
                async UniTask<T> GetTask()
                {
                    var service = ctx.Container.Resolve<IAddressableService>();
                    var go = await service.GetAsset<GameObject>(addressKey);
                    return go != null ? go.GetComponent<T>() : null;
                }
                return GetTask().Preserve();
            })
            .AsSingle();
    }
}