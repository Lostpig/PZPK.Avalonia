using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Material.Icons;
using PZPK.Desktop.Common;
using SukiUI.Controls;
using System;
using System.IO;


namespace PZPK.Desktop.Modules.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class OpenFilePanel : ComponentBase
{
    private StackPanel BuildOpenTab()
    {
        var primaryColor = App.Instance.Suki.GetSukiColor("SukiPrimaryColor");
        return VStackPanel(HorizontalAlignment.Stretch)
                .Children(
                    MaterialIcon(MaterialIconKind.BookAdd, 32)
                        .Foreground(primaryColor),
                    PzText("Open PZPK Notebook")
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
                                .OnClick(_ => SelectNotebookFile())
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
                    PzText("Create New Notebook")
                        .FontSize(20)
                        .Margin(0, 5, 0, 27)
                        .HorizontalAlignment(HorizontalAlignment.Center),
                    PzText("Path"),
                    Grid("*, Auto")
                        .Margin(0, 0, 0, 6)
                        .Children(
                            PzTextBox(() => CreatePath, v => CreatePath = v)
                                .IsReadOnly(true)
                                .Col(0),
                            SukiButton("Select")
                                .Margin(20, 0, 0, 0)
                                .OnClick(_ => SelectCreatePath())
                                .Col(1)
                        ),
                    PzText("Password"),
                    PzTextBox(() => CreatePw, v => CreatePw = v)
                        .Margin(0, 0, 0, 6)
                        .PasswordChar('*'),
                    PzText("Repeat password"),
                    PzTextBox(() => CreateRepeatPw, v => CreateRepeatPw = v)
                        .Margin(0, 0, 0, 6)
                        .PasswordChar('*'),
                    PzText(() => CreateErrorMessage)
                        .Foreground(new SolidColorBrush(Colors.Red))
                        .IsVisible(() => !string.IsNullOrWhiteSpace(CreateErrorMessage)),
                    SukiButton("Create", "Flat", "Rounded")
                        .Width(100)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Margin(0, 40, 0, 0)
                        .OnClick(_ => CreateNotebook())
                );
    }

    protected override object Build()
    {
        return new GlassCard()
            .Width(380)
            .Height(420)
            .Content(
                new TabControl().Items(
                    new TabItem().Header("Open").Content(BuildOpenTab()),
                    new TabItem().Header("Create").Content(BuildCreateTab())
                )
            );
    }

    public event Action? NotebookOpened;

    private string SelectedPath = "";
    private string Password = "";
    private string ErrorMessage = "";

    private string CreatePath = "";
    private string CreatePw = "";
    private string CreateRepeatPw = "";
    private string CreateErrorMessage = "";

    private async void SelectNotebookFile()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open PZPK Notebook",
            FileTypeFilter = [
                new("PZNT Files")
                {
                    Patterns = ["*.pznt"]
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
    private async void SelectCreatePath()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Create PZPK Notebook",
            DefaultExtension = "pznt",
        });

        if (file is not null)
        {
            var localPath = file.Path.LocalPath;
            if (File.Exists(localPath))
            {
                CreateErrorMessage = "File already exists.";
            }
            else
            {
                CreatePath = localPath;
            }

            StateHasChanged();
        }
    }

    private void OpenNotebook()
    {
        if (!string.IsNullOrWhiteSpace(SelectedPath) && !string.IsNullOrWhiteSpace(Password))
        {
            try
            {
                NoteBookModel.Open(SelectedPath, Password);
                NotebookOpened?.Invoke();

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

    private void CreateNotebook()
    {
        if (!string.IsNullOrWhiteSpace(CreatePath) && !string.IsNullOrWhiteSpace(CreatePw) && !string.IsNullOrWhiteSpace(CreateRepeatPw))
        {
            if (CreatePw != CreateRepeatPw)
            {
                CreateErrorMessage = "Passwords do not match.";
                StateHasChanged();
                return;
            }

            try
            {
                NoteBookModel.Create(CreatePath, CreatePw);
                NotebookOpened?.Invoke();

                // SelectedPath = string.Empty;
                // Password = string.Empty;
                CreateErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                CreateErrorMessage = ex.Message ?? "Unknown error";
                StateHasChanged();
            }
        }
    }
}

