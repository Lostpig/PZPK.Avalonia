using PZ.RxAvalonia;
using PZ.RxAvalonia.Reactive;

namespace PZPK.Desktop.Common;

public static class CommonExtensions
{
    public static List<T> Sorted<T>(this List<T> list, IComparer<T> comparer)
    {
        list.Sort(comparer);
        return list;
    }

    // DEBUG - move to PZ.RxAvalonia
    public static T ItemsSourceEx<T, TValue>(this T control, ReactiveList<TValue> list) where T : ItemsControl
    {
        return control._set(ItemsControl.ItemsSourceProperty!, obs: list.WhenChanged);
    }
}
