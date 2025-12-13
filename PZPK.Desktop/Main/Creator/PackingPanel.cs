using Avalonia.Platform.Storage;
using System.IO;
using System.Reactive.Linq;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

internal class PackingPanel : PZComponentBase
{
    protected override Control Build()
    {
        var packing = Model.PackingInfo;
        var notRuning = packing.Running.Select(x => !x);

        return VStackPanel(Avalonia.Layout.HorizontalAlignment.Center)
            .Children(
                new DockPanel().Height(40).Width(300)
                    .Children(
                        PzText("SaveTo:")
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                            .Dock(Dock.Left),
                        HStackPanel()
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                            .Dock(Dock.Right)
                            .Children(
                                SukiButton("Select")
                                    .IsEnabled(notRuning)
                                    .OnClick(_ => SelectSavePath())
                            )
                    ),
                PzTextBox(packing.SavePath)
                    .Margin(0, 10, 0, 0)
                    .Width(300)
                    .IsReadOnly(true),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText("Files:").Dock(Dock.Left),
                        PzText(packing.FilesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText("Bytes:").Dock(Dock.Left),
                        PzText(packing.BytesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new ProgressBar()
                    .Minimum(0)
                    .Maximum(100)
                    .Value(packing.Percent)
                    .Height(20)
                    .Width(480)
                    .Margin(0, 30, 0, 0),
                HStackPanel().HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .Margin(0, 30, 0, 0)
                    .Children(
                        SukiButton("Prev", "Accent", "Flat").Width(120)
                            .Margin(0, 0, 20, 0)
                            .IsEnabled(notRuning)
                            .OnClick(_ => Model.PreviousStep()),
                        SukiButton("Start").Width(120)
                            .IsVisible(notRuning)
                            .OnClick(_ => Model.Start()),
                        SukiButton("Cancel", "Danger").Width(120)
                            .IsVisible(packing.Running)
                            .OnClick(_ => CancelPacking())
                    )
            );
    }

    private static CreatorModel Model => CreatorModel.Instance;

    private async void SelectSavePath()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save PZPK Package",
            DefaultExtension = "pzpk",
        });

        if (file is not null)
        {
            var localPath = file.Path.LocalPath;
            if (File.Exists(localPath))
            {
                Model.Toast.Error("File already exists.");
            }
            else
            {
                Model.PackingInfo.SavePath.OnNext(localPath);
            }
        }
    }
    private async void CancelPacking()
    {
        var sure = await Model.Dialog.WarningConfirm("Are you sure you want to cancel packing?");

        if (sure)
        {
            Model.Cancel();
        }
    }
}
