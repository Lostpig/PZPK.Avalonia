using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;
using PZPK.Desktop.Modules.Global;

namespace PZPK.Desktop.Modules.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class ExplorerPage : ComponentBase
{
    protected override object Build()
    {
        return new Panel()
            .Children(
                new OpenFilePanel()
                    .IsVisible(() => Model is null)
                    .OnPackageOpened(OnPackageOpened),
                new ExplorerPanel()
                    .Ref(out ExpPanel)
                    .IsVisible(() => Model is not null)
                    .OnPackageClose(OnPackageClose)
            );
    }

    private PZPKPackageModel? Model => PZPKPackageModel.Current;
    private ExplorerPanel ExpPanel;
    private void OnPackageOpened()
    {
        ExpPanel?.UpdateModel();
        StateHasChanged();
    }
    private void OnPackageClose()
    {
        Model?.Close();
        StateHasChanged();
    }
}
