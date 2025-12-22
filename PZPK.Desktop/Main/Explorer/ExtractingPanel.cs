namespace PZPK.Desktop.Main.Explorer;

using Avalonia.Media;
using PZPK.Desktop.Common;
using System.Reactive.Linq;
using static PZPK.Desktop.Common.ControlHelpers;

public class ExtractingPanel: PZComponentBase
{
    private static StackPanel BuildContent()
    {
        var filesText = Model.ExtractProgress.Select(p => $"{p.ProcessedFiles}/{p.Files}");
        var bytesText = Model.ExtractProgress.Select(
            p => $"{Utility.ComputeFileSize(p.ProcessedBytes)}/{Utility.ComputeFileSize(p.Bytes)}"
        );
        var percent = Model.ExtractProgress.Select(p => Utility.ComputePercent(p.ProcessedBytes, p.Bytes));

        return VStackPanel(Avalonia.Layout.HorizontalAlignment.Center)
            .Children(
                PzText(() => LOC.PZPK.ExtractingDDD, "h3")
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText(() => LOC.PZPK.Files).Dock(Dock.Left),
                        PzText(filesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new DockPanel().Height(40).Width(300)
                    .Children(
                        PzText(() => LOC.PZPK.Bytes).Dock(Dock.Left),
                        PzText(bytesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new ProgressBar()
                    .Minimum(0)
                    .Maximum(100)
                    .Value(percent)
                    .Height(20)
                    .Width(360)
                    .Margin(0, 10, 0, 0),
                SukiButton(() => LOC.Base.Cancel, "Danger")
                    .Width(120)
                    .Margin(0, 20, 0, 0)
                    .OnClick(_ => CancelPacking())
            );
    }
    protected override Control Build()
    {
        var maskColor = App.Instance.Suki.GetSukiColor("SukiDialogBackground");
        var contentColor = App.Instance.Suki.GetSukiColor("SukiCardBackground");

        var content = new Border()
                .Background(contentColor)
                .CornerRadius(10)
                .Width(380)
                .Height(250)
                .Child(BuildContent());

        var mask = new Canvas()
            .IsHitTestVisible(false)
            .Background(maskColor)
            .Opacity(0.4);

        return new Panel()
            .Background(Brushes.Transparent)
            .Children(mask, content);
    }
    
    private static ExplorerModel Model => ExplorerModel.Instance;
    private static void CancelPacking()
    {
        Model.CancelExtracting();
    }
}
