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

public class EditorPanel: ComponentBase
{
    private readonly NoteBookModel Model;
    private TextEditor Editor { get; set; }
    private RegistryOptions RegOptions { get; set; }
    private TextMate.Installation TextMateInstallation { get; set; }
    private readonly List<FontFamily> Fonts = [.. FontManager.Current.SystemFonts.OrderBy(f => f.Name)];

    private Language SelectedLanguage { get; set; }
    private FontFamily Font { get; set; } = FontFamily.Parse("Consolas");
    private string ThemeText { get; set; } = Enum.GetName<ThemeName>(ThemeName.DarkPlus) ?? "DarkPlus";
    private readonly Dictionary<string, ThemeName> Themes = [];
    private int EditFontSize = 14;

    private string Title { get; set; } = "";
    private string Content { get; set; } = "";

    private static readonly int[] FontSizes = [12, 14, 16, 18, 20, 24, 28, 32, 40, 48, 56, 64, 72];

    public EditorPanel(NoteBookModel model) : base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        RegOptions = new RegistryOptions(ThemeName.DarkPlus);
        SelectedLanguage = RegOptions.GetLanguageByExtension(".md");

        Editor = new()
        {
            FontFamily = Font,
            FontSize = EditFontSize,
            Background = Brushes.Transparent,
            ShowLineNumbers = true
        };
        Editor.Options.ShowSpaces = true;
        Editor.FlowDirection = FlowDirection.LeftToRight;
        Editor.Resources.Add("TextAreaSelectionBrush", Brushes.DarkBlue);

        TextMateInstallation = Editor.InstallTextMate(RegOptions);
        var scopeName = RegOptions.GetScopeByLanguageId(SelectedLanguage.Id);
        TextMateInstallation.SetGrammar(scopeName);

        Initialize();

        Model.NoteChanged += OnNoteChanged;
    }

    protected override object Build()
    {
        foreach (var theme in Enum.GetValues<ThemeName>())
        {
            var name = Enum.GetName<ThemeName>(theme)!;
            Themes.Add(name, theme);
        }

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
                            .ItemsSource(FontSizes)
                            .SelectedItem(() => EditFontSize, v => EditFontSize = (int)v)
                            .OnSelectionChanged(_ => Editor.FontSize = EditFontSize)
                    ),
                new SukiUI.Controls.GlassCard()
                    .Row(2)
                    .Margin(30, 0, 30, 20)
                    .Content(
                        Editor
                    )
            );
    }

    private void OnNoteChanged()
    {
        var note = Model.Note;

        Title = note?.Title ?? "";
        Content = note?.Content ?? "";
        Editor.Text = Content;

        IsVisible = note != null;

        StateHasChanged();
    }
    private void SaveNote()
    {
        Content = Editor.Text;
        Model.ModifyNote(Title, Content);
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
        var ok = await Model.Dialog.DeleteConfirm("Sure to delete?");
        if (ok)
        {
            Model.DeleteNote();
        }
    }
}
