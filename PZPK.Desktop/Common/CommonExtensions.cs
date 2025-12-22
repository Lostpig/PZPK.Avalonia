using System.Reactive.Linq;

namespace PZPK.Desktop.Common;

public static class CommonExtensions
{
    public static List<T> Sorted<T>(this List<T> list, IComparer<T> comparer)
    {
        list.Sort(comparer);
        return list;
    }

    public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan time)
    {
        var last = DateTime.UtcNow;
        return source.Where(x =>
        {
            var elapsed = DateTime.UtcNow - last;
            if (elapsed > time)
            {
                last = DateTime.UtcNow;
                return true; 
            }

            return false;
        });
    }
}
