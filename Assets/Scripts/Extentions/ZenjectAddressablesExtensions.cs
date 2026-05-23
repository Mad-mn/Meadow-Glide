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
            .FromMethod(ctx => ctx.Container.Resolve<IAddressableService>().GetAsset<T>(addressKey))
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
            .FromMethod(async ctx =>
            {
                var service = ctx.Container.Resolve<IAddressableService>();
                var instance = await service.InstantiateAsync(addressKey);
                return instance != null ? instance.GetComponent<T>() : null;
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
    /// Метод для зворотної сумісності. Використовуйте BindAddressableComponent для нових біндінгів.
    /// </summary>
    public static void BindFromAddressablePrefab<T>(this DiContainer container, string addressKey) where T : Component
    {
        container.BindAddressableComponent<T>(addressKey);
    }
}