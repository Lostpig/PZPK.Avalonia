using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

internal class PackingPanel : PZComponentBase
{
    protected override object Build()
    {
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
                                    .IsEnabled(() => !Model.PackingInfo.Running)
                                    .OnClick(_ => SelectSavePath())
                            )
                    ),
                PzTextBox(() => Model.PackingInfo.SavePath, v => Model.PackingInfo.SavePath = v)
                    .Margin(0, 10, 0, 0)
                    .Width(300)
                    .IsReadOnly(true),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText("Files:").Dock(Dock.Left),
                        PzText(() => Model.PackingInfo.FilesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText("Bytes:").Dock(Dock.Left),
                        PzText(() => Model.PackingInfo.BytesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new ProgressBar()
                    .Minimum(0)
                    .Maximum(100)
                    .Value(() => Model.PackingInfo.Percent)
                    .Height(20)
                    .Width(480)
                    .Margin(0, 30, 0, 0),
                HStackPanel().HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .Margin(0, 30, 0, 0)
                    .Children(
                        SukiButton("Prev", "Accent", "Flat").Width(120)
                            .Margin(0, 0, 20, 0)
                            .IsEnabled(() => !Model.PackingInfo.Running)
                            .OnClick(_ => Model.PreviousStep()),
                        SukiButton("Start").Width(120)
                            .IsVisible(() => !Model.PackingInfo.Running)
                            .OnClick(_ => StartPacking()),
                        SukiButton("Cancel", "Danger").Width(120)
                            .IsVisible(() => Model.PackingInfo.Running)
                            .OnClick(_ => CancelPacking())
                    )
            );
    }
    public PackingPanel(CreatorModel model) : base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        Model.OnStepChanged += OnStepChanged;
        Model.OnPackingProgressed += OnPackingProgressed;

        Initialize();
    }

    private readonly CreatorModel Model;

    private void OnStepChanged()
    {
        if (Model.Step != 3) return;
        StateHasChanged();
    }
    private void OnPackingProgressed()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }

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
                Model.PackingInfo.SavePath = localPath;
            }

            StateHasChanged();
        }
    }

    private void StartPacking()
    {
        if (string.IsNullOrWhiteSpace(Model.PackingInfo.SavePath))
        {
            return;
        }

        Model.Start(Model.PackingInfo.SavePath);
        StateHasChanged();
    }
    private async void CancelPacking()
    {
        var sure = await Model.Dialog.WarningConfirm("Are you sure you want to cancel packing?");

        if (sure)
        {
            Model.Cancel();
            StateHasChanged();
        }
    }
}
