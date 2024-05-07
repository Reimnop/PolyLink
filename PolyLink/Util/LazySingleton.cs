using System;

namespace PolyLink.Util;

public static class LazySingleton<T> where T : new()
{
    private static Lazy<T> LazyInstance = new(() => new T());

    public static T Instance => LazyInstance.Value;
    
    public static void Reset()
    {
        LazyInstance = new Lazy<T>(() => new T());
    }
}