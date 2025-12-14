using System.Reactive.Linq;

namespace PZPK.Desktop.Main.Explorer;

public class ExplorerPage : PZComponentBase
{
    protected override Control Build()
    {
        return new Panel()
            .Children(
                new ExtractingPanel()
                    .ZIndex(99)
                    .IsVisible(Model.IsExtracting),
                new OpenFilePanel()
                    .IsVisible(Model.Package.Select(p => p is null)),
                new ExplorerPanel()
                    .IsVisible(Model.Package.Select(p => p is not null))
            );
    }

    private readonly ExplorerModel Model = ExplorerModel.Instance;
}
