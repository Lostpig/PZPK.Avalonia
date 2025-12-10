using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;
using System;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class CompletePanel : PZComponentBase
{
    protected override object Build()
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
                        PzTextBox(() => Model.CompleteInfo.PackagePath)
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
                        PzText(() => Model.CompleteInfo.FilesCount.ToString(), "Primary"),
                        PzText(" files in "),
                        PzText(() => Model.CompleteInfo.UsedTime.ToString(@"hh\:mm\:ss"), "Primary")
                    ),
                HStackPanel()
                    .Margin(0, 10, 0, 0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .Children(
                        PzText("Total Size "),
                        PzText(() => Utility.ComputeFileSize(Model.CompleteInfo.PackageSize), "Primary"),
                        PzText(", process speed "),
                        PzText(() => Utility.ComputeFileSize(Model.CompleteInfo.Speed), "Primary"),
                        PzText("/S")
                    ),
                SukiButton("Done", "Flat")
                    .Width(100)
                    .Margin(0, 30, 0, 0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .OnClick(_ => Model.Reset())
            );
    }

    private readonly CreatorModel Model;
    public CompletePanel(CreatorModel model) : base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        Initialize();
    }
    private void OpenDirectory()
    {
        var path = System.IO.Path.GetDirectoryName(Model.CompleteInfo.PackagePath);
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
