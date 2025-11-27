using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Material.Icons;
using PZPK.Desktop.Common;
using PZPK.Desktop.Modules.Global;
using SukiUI.Controls;
using System;

namespace PZPK.Desktop.Modules.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class OpenFilePanel : ComponentBase
{
    protected override object Build()
    {
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
                        PzText(() => ErrorMessage)
                            .Foreground(new SolidColorBrush(Colors.Red))
                            .IsVisible(() => !string.IsNullOrWhiteSpace(ErrorMessage)),
                        SukiButton("Open", "Flat", "Rounded")
                            .Width(100)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Margin(0, 40, 0, 0)
                            .OnClick(_ => OpenPackage())
                    )
            );
    }

    public event Action? PackageOpened;

    private string SelectedPath = "";
    private string Password = "";
    private string ErrorMessage = "";

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
        if (!string.IsNullOrWhiteSpace(SelectedPath) && !string.IsNullOrWhiteSpace(Password))
        {
            try
            {
                PZPKPackageModel.Open(SelectedPath, Password);
                PackageOpened?.Invoke();

                // SelectedPath = string.Empty;
                // Password = string.Empty;
                ErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message ?? "Unknown error";
                StateHasChanged();
            }
        }
    }
}
