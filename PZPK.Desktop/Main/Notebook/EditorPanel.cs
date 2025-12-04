using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Platform;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using PZPK.Desktop.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using TextMateSharp.Grammars;

namespace PZPK.Desktop.Main.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class EditorPanel(NoteBookModel vm): ComponentBase<NoteBookModel>(vm)
{
    private TextEditor BuildEditor()
    {
        RegOptions = new RegistryOptions(ThemeName.DarkPlus);
        SelectedLanguage = RegOptions.GetLanguageByExtension(".md");

        Editor = new TextEditor();
        Editor.FontFamily = Font;
        Editor.FontSize = EditFontSize;
        Editor.Background = Brushes.Transparent;
        Editor.ShowLineNumbers = true;
        Editor.Options.ShowSpaces = true;
        Editor.FlowDirection = FlowDirection.LeftToRight;
        Editor.Resources.Add("TextAreaSelectionBrush", Brushes.DarkBlue);
        // Editor.Options.HighlightCurrentLine = true;

        return Editor;
    }
    protected override object Build(NoteBookModel? vm)
    {
        InitializeModel();
        foreach (var theme in Enum.GetValues<ThemeName>())
        {
            var name = Enum.GetName<ThemeName>(theme)!;
            Themes.Add(name, theme);
        }

        var editor = BuildEditor();
        return Grid(null, "80, 45, 1*").
            Children(
                Grid("*, 200")
                    .Row(0)
                    .Margin(30, 20)
                    .Children(
                        PzTextBox(() => Title, v => Title = v).Col(0),
                        HStackPanel()
                            .Col(1)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Children(
                                SukiButton("Save").Margin(10, 0).OnClick(_ => SaveNote()),
                                SukiButton("Delete", "Accent").Margin(10, 0).OnClick(_ => OnDelete())
                            )
                    ),
                HStackPanel()
                    .Row(1)
                    .Margin(30, 0, 30, 5)
                    .Children(
                        new ComboBox()
                            .ItemsSource(RegOptions.GetAvailableLanguages())
                            .SelectedItem(() => SelectedLanguage, v => SelectedLanguage = (Language)v)
                            .OnSelectionChanged(_ => UpdateLanguage()),
                        new ComboBox()
                            .ItemsSource(Themes.Keys)
                            .SelectedItem(() => ThemeText, v => ThemeText = (string)v)
                            .OnSelectionChanged(_ => UpdateTheme()),
                        new ComboBox()
                            .ItemsSource(Fonts)
                            .SelectedItem(() => Font, v => Font = (FontFamily)v)
                            .OnSelectionChanged(_ => UpdateFont()),
                        new ComboBox()
                            .ItemsSource(new int[] { 12, 14, 16, 18, 20, 24, 28, 32, 40, 48, 56, 64, 72 })
                            .SelectedItem(() => EditFontSize, v => EditFontSize = (int)v)
                            .OnSelectionChanged(_ => editor.FontSize = EditFontSize)
                    ),
                new SukiUI.Controls.GlassCard()
                    .Row(2)
                    .Margin(30, 0, 30, 20)
                    .Content(
                        editor
                    )
            );
    }
    protected override void OnAfterInitialized()
    {
        base.OnAfterInitialized();
        TextMateInstallation = Editor.InstallTextMate(RegOptions);
        var scopeName = RegOptions.GetScopeByLanguageId(SelectedLanguage.Id);
        TextMateInstallation.SetGrammar(scopeName);
    }

    private TextEditor Editor;
    private RegistryOptions RegOptions;
    private TextMate.Installation TextMateInstallation;
    private List<FontFamily> Fonts = FontManager.Current.SystemFonts.OrderBy(f => f.Name).ToList();

    private Language SelectedLanguage { get; set; }
    private FontFamily Font { get; set; } = FontFamily.Parse("Consolas");
    private string ThemeText { get; set; } = Enum.GetName<ThemeName>(ThemeName.DarkPlus) ?? "DarkPlus";
    private Dictionary<string, ThemeName> Themes = new();
    private int EditFontSize = 14;

    private string Title { get; set; } = "";
    private string Content { get; set; } = "";

    private void InitializeModel()
    {
        ViewModel?.NoteChanged += OnNoteChanged;
    }
    private void OnNoteChanged()
    {
        var note = ViewModel?.Note;

        Title = note?.Title ?? "";
        Content = note?.Content ?? "";
        Editor.Text = Content;

        IsVisible = note != null;

        StateHasChanged();
    }
    private void SaveNote()
    {
        Content = Editor.Text;
        ViewModel?.ModifyNote(Title, Content);
    }
    private void UpdateLanguage()
    {
        var scopeName = RegOptions.GetScopeByLanguageId(SelectedLanguage.Id);
        TextMateInstallation.SetGrammar(scopeName);
    }
    private void UpdateTheme()
    {
        var theme = Themes[ThemeText];

        RegOptions = new RegistryOptions(theme);
        TextMateInstallation = Editor.InstallTextMate(RegOptions);
        var scopeName = RegOptions.GetScopeByLanguageId(SelectedLanguage.Id);
        TextMateInstallation.SetGrammar(scopeName);
    }
    private void UpdateFont()
    {
        Editor.FontFamily = Font;
    }
    private async void OnDelete()
    {
        var ok = await Dialog.Confirm("Sure to delete?");
        if (ok)
        {
            ViewModel?.DeleteNote();
        }
    }
}
