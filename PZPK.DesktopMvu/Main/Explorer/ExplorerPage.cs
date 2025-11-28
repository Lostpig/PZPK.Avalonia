using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;

namespace PZPK.Desktop.Modules.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class ExplorerPage : ComponentBase
{
    protected override object Build()
    {
        _model.OnPackageOpened += StateHasChanged;
        _model.OnPackageClosed += StateHasChanged;

        return new Panel()
            .Children(
                new OpenFilePanel(_model)
                    .IsVisible(() => _model.Model is null),
                new ExplorerPanel(_model)
                    .IsVisible(() => _model.Model is not null)
            );
    }

    private ExplorerModel _model = new();
}
