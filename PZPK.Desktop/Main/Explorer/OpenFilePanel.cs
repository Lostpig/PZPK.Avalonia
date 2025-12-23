using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Material.Icons;
using SukiUI.Controls;

namespace PZPK.Desktop.Main.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class OpenFilePanel : PZComponentBase
{
    protected override Control Build()
    {
        return new GlassCard()
            .Width(380)
            .Height(360)
            .Content(
                VStackPanel(HorizontalAlignment.Stretch)
                    .Children(
                        MaterialIcon(MaterialIconKind.FolderOpen, 48)
                            .Foreground(() => Suki.GetSukiColor("SukiPrimaryColor")),
                        PzText(() => LOC.Message.OpenPZPKFile)
                            .FontSize(20)
                            .Margin(0, 5, 0, 27)
                            .HorizontalAlignment(HorizontalAlignment.Center),
                        PzText(() => LOC.Base.File),
                        Grid("*, Auto")
                            .Margin(0, 0, 0, 6)
                            .Children(
                                PzTextBox(Model.FilePath)
                                    .IsReadOnly(true)
                                    .Col(0),
                                SukiButton(() => LOC.Base.Select)
                                    .Margin(20, 0, 0, 0)
                                    .OnClick(_ => SelectPackageFile())
                                    .Col(1)
                            ),
                        PzText(() => LOC.Base.Password),
                        PzTextBox(Model.Password)
                            .Margin(0, 0, 0, 6)
                            .PasswordChar('*'),
                        SukiButton(() => LOC.Base.Open, "Flat", "Rounded")
                            .Width(100)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Margin(0, 40, 0, 0)
                            .OnClick(_ => Model.OpenPackage())
                    )
            );
    }

    private static ExplorerModel Model => ExplorerModel.Instance;

    private async void SelectPackageFile()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = LOC.Message.OpenPZPKFile,
            FileTypeFilter = [
                new(LOC.PZPK.PZPKFile)
                {
                    Patterns = ["*.pzpk"]
                }
            ],
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            Model.FilePath.OnNext(files[0].Path.LocalPath);
        }
    }
}
