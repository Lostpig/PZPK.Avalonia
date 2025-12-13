using PZPK.Desktop.Common;
using System.Reactive.Linq;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class CompletePanel : PZComponentBase
{
    protected override Control Build()
    {
        return VStackPanel(Avalonia.Layout.HorizontalAlignment.Center)
            .Width(400)
            .Children(
                PzText("Packing Complete!", "h2")
                    .Margin(0, 0, 0, 30)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),
                PzText("File saved to: ")
                    .Margin(0, 0, 0, 10)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left),
                Grid("*, Auto").Children(
                        PzReadOnlyTextBox(Model.Completed.Select(c => c.PackagePath))
                            .Ref(out pathBox)
                            .Col(0),
                        SukiButton("Open Directory")
                            .Col(1)
                            .Margin(10, 0, 0, 0)
                            .OnClick(_ => OpenDirectory())
                    ),
                HStackPanel()
                    .Margin(0, 30, 0, 0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .Children(
                        PzText("Packing "),
                        PzText(Model.Completed.Select(c => c.Count.ToString()), "Primary"),
                        PzText(" files in "),
                        PzText(Model.Completed.Select(c => c.UsedTime.ToString(@"hh\:mm\:ss")), "Primary")
                    ),
                HStackPanel()
                    .Margin(0, 10, 0, 0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .Children(
                        PzText("Total Size "),
                        PzText(Model.Completed.Select(c => c.Size).Select(Utility.ComputeFileSize), "Primary"),
                        PzText(", process speed "),
                        PzText(Model.Completed.Select(c => c.Speed).Select(Utility.ComputeFileSize), "Primary"),
                        PzText("/S")
                    ),
                SukiButton("Done", "Flat")
                    .Width(100)
                    .Margin(0, 30, 0, 0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .OnClick(_ => Model.Reset())
            );
    }

    private TextBox pathBox;
    private static CreatorModel Model => CreatorModel.Instance;

    private void OpenDirectory()
    {
        var filePath = pathBox?.Text;
        if (string.IsNullOrWhiteSpace(filePath)) return;

        var path = System.IO.Path.GetDirectoryName(filePath);
        if (path is not null && System.IO.Directory.Exists(path))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
