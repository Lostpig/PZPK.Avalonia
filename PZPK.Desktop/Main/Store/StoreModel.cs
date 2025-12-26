using System.Reactive;
using System.Reactive.Subjects;

namespace PZPK.Desktop.Main.Store;

internal enum StoreRouteState
{
    None,
    Explore,
    Detail,
    Variant,
    Tags
}


internal class StoreModel : PageModelBase
{
    public class StoreRoute
    {
        public StoreRouteState State { get; private set; } = StoreRouteState.None;
        public int ParamInt { get; private set; } = 0;
        public string ParamStr { get; private set; } = "";
    }

    private static StoreModel? _instance;
    public static StoreModel Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }


    public StoreRoute Route { get; private set; }
    public Subject<Unit> RouteChange { get; init; } = new();

    private StoreModel()
    {
        Route = new();
    }
}
