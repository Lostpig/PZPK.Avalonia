using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;
using System.Threading.Tasks;

namespace PZPK.Desktop.Main.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class ExtractingPanel: PZComponentBase
{
    private StackPanel BuildContent()
    {
        return VStackPanel(Avalonia.Layout.HorizontalAlignment.Center)
            .Children(
                PzText("Extracting...", "h3")
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText("Files:").Dock(Dock.Left),
                        PzText(() => Model.ExtractingState.FilesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new DockPanel().Height(40).Width(300)
                    .Children(
                        PzText("Bytes:").Dock(Dock.Left),
                        PzText(() => Model.ExtractingState.BytesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new ProgressBar()
                    .Minimum(0)
                    .Maximum(100)
                    .Value(() => Model.ExtractingState.Percent)
                    .Height(20)
                    .Width(360)
                    .Margin(0, 10, 0, 0),
                SukiButton("Cancel", "Danger")
                    .Width(120)
                    .Margin(0, 20, 0, 0)
                    .OnClick(_ => CancelPacking())
            );
    }
    protected override object Build()
    {
        var maskColor = App.Instance.Suki.GetSukiColor("SukiDialogBackground");
        var contentColor = App.Instance.Suki.GetSukiColor("SukiCardBackground");

        var content = new Border()
                .Background(contentColor)
                .CornerRadius(10)
                .Width(380)
                .Height(250)
                .Child(BuildContent());

        var mask = new Panel()
            .Background(maskColor)
            .Children(content);

        return mask;
    }

    public ExtractingPanel(ExplorerModel model): base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        Model.OnExtractingProgressed += StateHasChanged;

        Initialize();
    }
    
    private readonly ExplorerModel Model;
    private void CancelPacking()
    {
        Model.CancelExtracting();
    }
}
