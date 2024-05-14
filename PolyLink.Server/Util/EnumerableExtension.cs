namespace PolyLink.Server.Util;

public static class EnumerableExtension
{
    public static IEnumerable<(int Index, T Value)> Indexed<T>(this IEnumerable<T> source)
    {
        var index = 0;
        foreach (var value in source)
            yield return (index++, value);
    }
}