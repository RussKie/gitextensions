using System.ComponentModel.Design;

namespace System;

public static class ServiceExtensions
{
    /// <summary>
    ///  Gets the service object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the service object to get.</typeparam>
    /// <param name="provider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    /// <returns>A service object of type <typeparamref name="T"/> or <see langword="null"/> if there is no such service.</returns>
    internal static T? GetService<T>(this IServiceProvider provider)
        where T : class
        => provider.GetService(typeof(T)) as T;

    internal static void AddService<T>(this IServiceContainer container, object serviceInstance)
        where T : class
        => container.AddService(typeof(T), serviceInstance);

    internal static void AddService<T>(this IServiceContainer container, object serviceInstance, bool promote)
        where T : class
        => container.AddService(typeof(T), serviceInstance, promote);

    internal static void AddService<T>(this IServiceContainer container, ServiceCreatorCallback callback)
        where T : class
        => container.AddService(typeof(T), callback);

    internal static void AddService<T>(this IServiceContainer container, ServiceCreatorCallback callback, bool promote)
        where T : class
        => container.AddService(typeof(T), callback, promote);

    internal static void RemoveService<T>(this IServiceContainer container)
        where T : class
        => container.RemoveService(typeof(T));

    internal static void RemoveService<T>(this IServiceContainer container, bool promote)
        where T : class
        => container.RemoveService(typeof(T), promote);
}
