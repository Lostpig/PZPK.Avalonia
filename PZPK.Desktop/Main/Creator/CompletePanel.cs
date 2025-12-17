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
                PzText(() => LOC.PZPK.PackingCompleted, "h2")
                    .Margin(0, 0, 0, 30)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),
                PzText(() => LOC.Base.File)
                    .Margin(0, 0, 0, 10)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left),
                Grid("*, Auto").Children(
                        PzReadOnlyTextBox(Model.Completed.Select(c => c.PackagePath))
                            .Col(0),
                        SukiButton(() => LOC.PZPK.OpenDirectory)
                            .Col(1)
                            .Margin(10, 0, 0, 0)
                            .OnClick(_ => OpenDirectory())
                    ),
                Grid("*, Auto", "Auto, Auto, Auto, Auto")
                    .Margin(0, 30, 0, 0)
                    .Children(
                        PzText(() => LOC.PZPK.Files).Cell(0, 0),
                        PzText(Model.Completed.Select(c => c.Count.ToString())).Cell(1,0),
                        PzText(() => LOC.PZPK.Bytes).Cell(0, 1),
                        PzText(Model.Completed.Select(c => Utility.ComputeFileSize(c.Size))).Cell(1, 1),
                        PzText(() => LOC.PZPK.Time).Cell(0, 2),
                        PzText(Model.Completed.Select(c => c.UsedTime.ToString(@"hh\:mm\:ss"))).Cell(1, 2),
                        PzText(() => LOC.PZPK.Speed).Cell(0, 3),
                        PzText(Model.Completed.Select(c => Utility.ComputeFileSize(c.Speed) + "/s")).Cell(1, 3)
                    ),
                SukiButton(LOC.Base.Done, "Flat")
                    .Width(100)
                    .Margin(0, 30, 0, 0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .OnClick(_ => Model.Done())
            );
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return [
            Model.Completed.Subscribe(c => _savePath = c.PackagePath)
        ];
    }

    private string? _savePath;
    private static CreatorModel Model => CreatorModel.Instance;

    private void OpenDirectory()
    {
        var filePath = _savePath;
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
