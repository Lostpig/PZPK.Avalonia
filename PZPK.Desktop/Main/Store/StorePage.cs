namespace PZPK.Desktop.Main.Store;

internal class StorePage : PZComponentBase
{
    protected override Control Build()
    {
        return new Panel()
            .Children(
                new OpenStorePanel()
                    .IsVisible(() => Model.Route.State == StoreRouteState.None)
            );
    }

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return [
                Model.RouteChange.Subscribe(_ => UpdateState())
            ];
    }

    private static StoreModel Model => StoreModel.Instance;
}
