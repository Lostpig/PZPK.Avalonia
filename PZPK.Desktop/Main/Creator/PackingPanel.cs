using Avalonia.Media;
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
                        PzText(() => LOC.PZPK.SaveTo)
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                            .Dock(Dock.Left),
                        HStackPanel()
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Center)
                            .Dock(Dock.Right)
                            .Children(
                                SukiButton(() => LOC.Base.Select)
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
                        PzText(() => LOC.PZPK.Files).Dock(Dock.Left),
                        PzText(packing.FilesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText(() => LOC.PZPK.Bytes).Dock(Dock.Left),
                        PzText(packing.BytesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new DockPanel().Height(20).Width(300).Margin(0, 30, 0, 0)
                    .Children(
                        PzText(packing.TimerText).Dock(Dock.Left),
                        PzText(packing.SpeedText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                PzText(packing.Percent.Select(p => $"{p:f1}%"))
                    .Margin(0, 20, 0, 0)
                    .TextAlignment(TextAlignment.Center)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center),
                new ProgressBar()
                    .Height(20).Width(480)
                    .Minimum(0).Maximum(100)
                    .Value(packing.Percent),
                HStackPanel().HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .Margin(0, 30, 0, 0)
                    .Spacing(30)
                    .Children(
                        SukiButton(() => LOC.Base.Prev, "Accent", "Flat").Width(120)
                            .IsEnabled(notRuning)
                            .OnClick(_ => Model.PreviousStep()),
                        SukiButton(() => LOC.PZPK.Packing).Width(120)
                            .IsVisible(notRuning)
                            .OnClick(_ => Model.Start()),
#if DEBUG
                        SukiButton("Dev Start").Width(120)
                            .IsVisible(notRuning)
                            .OnClick(_ => Model.DebugStart()),
#endif
                        SukiButton(() => LOC.Base.Cancel, "Danger").Width(120)
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
            Title = LOC.PZPK.SavePZPKFile,
            DefaultExtension = "pzpk",
        });

        if (file is not null)
        {
            var localPath = file.Path.LocalPath;
            if (File.Exists(localPath))
            {
                Model.Toast.Error(LOC.Error.FileExistsed);
            }
            else
            {
                Model.PackingInfo.SavePath.OnNext(localPath);
            }
        }
    }
    private static async void CancelPacking()
    {
        var opt = PZDialog.ConfirmOptions(LOC.Base.Warning, LOC.Message.SureToCancelPacking);
        var sure = await Model.Dialog.ShowDialog(opt);

        if (sure)
        {
            Model.Cancel();
        }
    }
}
