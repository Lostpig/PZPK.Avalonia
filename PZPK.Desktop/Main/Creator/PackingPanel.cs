using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using System;
using System.IO;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

internal class PackingPanel(CreatorModel vm) : ComponentBase<CreatorModel>(vm)
{
    protected override object Build(CreatorModel? vm)
    {
        if (vm is null) throw new InvalidOperationException("ViewModel cannot be null");

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
                                    .IsEnabled(() => !vm.PackingInfo.Running)
                                    .OnClick(_ => SelectSavePath())
                            )
                    ),
                PzTextBox(() => SavePath, v => SavePath = v)
                    .Margin(0, 10, 0, 0)
                    .Width(300)
                    .IsReadOnly(true),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText("Files:").Dock(Dock.Left),
                        PzText(() => vm.PackingInfo.FilesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new DockPanel().Height(40).Width(300).Margin(0, 10, 0, 0)
                    .Children(
                        PzText("Bytes:").Dock(Dock.Left),
                        PzText(() => vm.PackingInfo.BytesText)
                            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                    ),
                new ProgressBar()
                    .Minimum(0)
                    .Maximum(100)
                    .Value(() => vm.PackingInfo.Percent)
                    .Height(20)
                    .Width(480)
                    .Margin(0, 30, 0, 0),
                HStackPanel().HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Center)
                    .Margin(0, 30, 0, 0)
                    .Children(
                        SukiButton("Start").Width(120)
                            .IsVisible(() => !vm.PackingInfo.Running)
                            .OnClick(_ => StartPacking()),
                        SukiButton("Cancel", "Flat", "Accent").Width(120)
                            .IsVisible(() => vm.PackingInfo.Running)
                            .OnClick(_ => CancelPacking()),
                        SukiButton("Prev", "Accent").Width(120)
                            .Margin(20, 0, 0, 0)
                            .IsEnabled(() => !vm.PackingInfo.Running)
                            .OnClick(_ => Prev())
                    )
            );
    }
    protected override void OnCreated()
    {
        base.OnCreated();

        ViewModel?.OnStepChanged += OnStepChanged;
        ViewModel?.OnPackingProgressed += OnPackingProgressed;
    }

    private void OnStepChanged()
    {
        if (ViewModel?.Step != 3) return;
        StateHasChanged();
    }
    private void OnPackingProgressed()
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            StateHasChanged();
        });
    }

    private string SavePath { get; set; } = "";
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
                Toast.Error("File already exists.");
            }
            else
            {
                SavePath = localPath;
            }

            StateHasChanged();
        }
    }

    private void StartPacking()
    {
        if (string.IsNullOrWhiteSpace(SavePath))
        {
            return;
        }

        ViewModel?.Start(SavePath);
        StateHasChanged();
    }
    private void CancelPacking()
    {
        ViewModel?.Cancel();
        StateHasChanged();
    }
    private void Prev()
    {
        ViewModel?.PreviousStep();
    }
}
