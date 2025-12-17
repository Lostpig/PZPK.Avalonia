using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Material.Icons;
using SukiUI.Controls;
using System.IO;
using System.Reactive.Subjects;


namespace PZPK.Desktop.Main.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class OpenFilePanel : PZComponentBase
{
    private StackPanel BuildOpenTab()
    {
        var primaryColor = App.Instance.Suki.GetSukiColor("SukiPrimaryColor");
        return VStackPanel(HorizontalAlignment.Stretch)
                .Children(
                    MaterialIcon(MaterialIconKind.BookAdd, 32)
                        .Foreground(primaryColor),
                    PzText(() => LOC.Message.OpenPZPKNotebook)
                        .FontSize(20)
                        .Margin(0, 5, 0, 27)
                        .HorizontalAlignment(HorizontalAlignment.Center),
                    PzText(() => LOC.Base.File),
                    Grid("*, Auto")
                        .Margin(0, 0, 0, 6)
                        .Children(
                            PzTextBox(SelectedPath)
                                .IsReadOnly(true)
                                .Col(0),
                            SukiButton(() => LOC.Base.Select)
                                .Margin(20, 0, 0, 0)
                                .OnClick(_ => SelectNotebookFile())
                                .Col(1)
                        ),
                    PzText(() => LOC.Base.Password),
                    PzTextBox(Password)
                        .Margin(0, 0, 0, 6)
                        .PasswordChar('*'),
                    SukiButton(() => LOC.Base.Open, "Flat", "Rounded")
                        .Width(100)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Margin(0, 40, 0, 0)
                        .OnClick(_ => OpenNotebook())
                );
    }
    private StackPanel BuildCreateTab()
    {
        var primaryColor = App.Instance.Suki.GetSukiColor("SukiPrimaryColor");
        return VStackPanel(HorizontalAlignment.Stretch)
                .Children(
                    MaterialIcon(MaterialIconKind.BookEdit, 32)
                        .Foreground(primaryColor),
                    PzText(() => LOC.Message.CreatePZPKNotebook)
                        .FontSize(20)
                        .Margin(0, 5, 0, 27)
                        .HorizontalAlignment(HorizontalAlignment.Center),
                    PzText(() => LOC.PZPK.SaveTo),
                    Grid("*, Auto")
                        .Margin(0, 0, 0, 6)
                        .Children(
                            PzTextBox(CreatePath)
                                .IsReadOnly(true)
                                .Col(0),
                            SukiButton(() => LOC.Base.Select)
                                .Margin(20, 0, 0, 0)
                                .OnClick(_ => SelectCreatePath())
                                .Col(1)
                        ),
                    PzText(() => LOC.Base.Password),
                    PzTextBox(CreatePw)
                        .Margin(0, 0, 0, 6)
                        .PasswordChar('*'),
                    PzText(() => LOC.Base.RepeatPassword),
                    PzTextBox(CreateRepeatPw)
                        .Margin(0, 0, 0, 6)
                        .PasswordChar('*'),
                    SukiButton(() => LOC.Base.Create, "Flat", "Rounded")
                        .Width(100)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Margin(0, 40, 0, 0)
                        .OnClick(_ => CreateNotebook())
                );
    }
    protected override Control Build()
    {
        return new GlassCard()
            .Width(380)
            .Height(450)
            .Content(
                new TabControl().Items(
                    new TabItem().Header(() => LOC.Base.Open).Content(BuildOpenTab()),
                    new TabItem().Header(() => LOC.Base.Create).Content(BuildCreateTab())
                )
            );
    }

    private static NoteBookModel Model => NoteBookModel.Instance;
    private readonly BehaviorSubject<string> SelectedPath = new("");
    private readonly BehaviorSubject<string> Password = new("");
    private readonly BehaviorSubject<string> CreatePath = new("");
    private readonly BehaviorSubject<string> CreatePw = new("");
    private readonly BehaviorSubject<string> CreateRepeatPw = new("");

    private async void SelectNotebookFile()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = LOC.Message.OpenPZPKNotebook,
            FileTypeFilter = [
                new(LOC.PZPK.PZPKNotebook)
                {
                    Patterns = ["*.pznt"]
                }
            ],
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            SelectedPath.OnNext(files[0].Path.LocalPath);
        }
    }
    private async void SelectCreatePath()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = LOC.Message.CreatePZPKNotebook,
            DefaultExtension = "pznt",
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
                CreatePath.OnNext(localPath);
            }
        }
    }

    private void OpenNotebook()
    {
        Model.Open(SelectedPath.Value, Password.Value);
    }
    private void CreateNotebook()
    {
        Model.Create(CreatePath.Value, CreatePw.Value, CreateRepeatPw.Value);
    }
}

