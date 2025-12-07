using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;
using System;
using System.Linq;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class CompletePanel(CreatorModel vm) : ComponentBase<CreatorModel>(vm)
{
    protected override object Build(CreatorModel? vm)
    {
        if (vm is null) throw new InvalidOperationException("ViewModel cannot be null");

        return VStackPanel(Avalonia.Layout.HorizontalAlignment.Center)
            .Width(400)
            .Children(
                PzText("Packing Complete!")
                    .FontSize(24)
                    .Margin(0, 0, 0, 30)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),
                PzText("File saved to: )")
                    .Margin(0, 0, 0, 10)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Left),
                Grid("*, Auto").Children(
                        PzTextBox(() => vm.CompleteInfo.PackagePath)
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
                        PzText(() => vm.CompleteInfo.FilesCount.ToString()),
                        PzText(" files in "),
                        PzText(() => vm.CompleteInfo.UsedTime.ToString(@"hh\:mm\:ss"))
                    ),
                HStackPanel()
                    .Margin(0, 10, 0, 0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .Children(
                        PzText("Total Size "),
                        PzText(() => Utility.ComputeFileSize(vm.CompleteInfo.PackageSize)),
                        PzText(", process speed "),
                        PzText(() => Utility.ComputeFileSize(vm.CompleteInfo.Speed)),
                        PzText("/S")
                    ),
                SukiButton("Done")
                    .Width(100)
                    .Margin(0, 30, 0, 0)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .OnClick(_ => vm.Reset())
            );
    }

    private void OpenDirectory()
    {
        var path = System.IO.Path.GetDirectoryName(vm.CompleteInfo.PackagePath);
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
