using System.Collections.Generic;

namespace PolyLink.Util;

public static class Il2CppEnumerableExtension
{
    public static IEnumerable<T> ToEnumerable<T>(this Il2CppSystem.Collections.Generic.List<T> list)
    {
        for (var i = 0; i < list.Count; i++)
            yield return list[i];
    }
}