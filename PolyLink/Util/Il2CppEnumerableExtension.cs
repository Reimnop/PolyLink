using System.Collections.Generic;

namespace PolyLink.Util;

public static class Il2CppEnumerableExtension
{
    public static IEnumerable<T> ToEnumerable<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        for (var i = 0; i < list.Count; i++)
            yield return list[i];
    }

    public static IEnumerable<(int Index, T Value)> Indexed<T>(this IEnumerable<T> enumerable)
    {
        var index = 0;
        foreach (var value in enumerable)
            yield return (index++, value);
    }
}