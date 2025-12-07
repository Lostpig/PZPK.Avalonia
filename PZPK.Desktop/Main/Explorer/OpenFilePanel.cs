using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Platform.Storage;
using Material.Icons;
using PZPK.Desktop.Common;
using SukiUI.Controls;
using System;

namespace PZPK.Desktop.Main.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class OpenFilePanel(ExplorerModel vm) : ComponentBase<ExplorerModel>(vm)
{
    protected override object Build(ExplorerModel? vm)
    {
        if (vm is null) throw new InvalidOperationException("ViewModel cannot be null");
        var primaryColor = App.Instance.Suki.GetSukiColor("SukiPrimaryColor");

        return new GlassCard()
            .Width(380)
            .Height(360)
            .Content(
                VStackPanel(HorizontalAlignment.Stretch)
                    .Children(
                        MaterialIcon(MaterialIconKind.FolderOpen, 48)
                            .Foreground(primaryColor),
                        PzText("Open PZPK file")
                            .FontSize(20)
                            .Margin(0, 5, 0, 27)
                            .HorizontalAlignment(HorizontalAlignment.Center),
                        PzText("File"),
                        Grid("*, Auto")
                            .Margin(0, 0, 0, 6)
                            .Children(
                                PzTextBox(() => SelectedPath, v => SelectedPath = v)
                                    .IsReadOnly(true)
                                    .Col(0),
                                SukiButton("Select")
                                    .Margin(20, 0, 0, 0)
                                    .OnClick(_ => SelectPackageFile())
                                    .Col(1)
                            ),
                        PzText("Password"),
                        PzTextBox(() => Password, v => Password = v)
                            .Margin(0, 0, 0, 6)
                            .PasswordChar('*'),
                        SukiButton("Open", "Flat", "Rounded")
                            .Width(100)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Margin(0, 40, 0, 0)
                            .OnClick(_ => OpenPackage())
                    )
            );
    }

    private string SelectedPath { get; set; } = "";
    private string Password { get; set; } = "";

    private async void SelectPackageFile()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open PZPK File",
            FileTypeFilter = [
                new("PZPK Files")
                {
                    Patterns = ["*.pzpk"]
                }
            ],
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            SelectedPath = files[0].Path.LocalPath;
            StateHasChanged();
        }
    }
    private void OpenPackage()
    {
        ViewModel?.OpenPackage(SelectedPath, Password);
    }
}
