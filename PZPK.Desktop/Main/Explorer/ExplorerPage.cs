using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;

namespace PZPK.Desktop.Main.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class ExplorerPage : ComponentBase
{
    protected override object Build()
    {
        Model.OnPackageOpened += StateHasChanged;
        Model.OnPackageClosed += StateHasChanged;
        Model.OnExtractingChanged += _ => StateHasChanged();

        return new Panel()
            .Children(
                new ExtractingPanel(Model)
                    .ZIndex(99)
                    .IsVisible(() => Model.Extracting),
                new OpenFilePanel(Model)
                    .IsVisible(() => Model.Package is null),
                new ExplorerPanel(Model)
                    .IsVisible(() => Model.Package is not null)
            );
    }

    private readonly ExplorerModel Model = new();
}
