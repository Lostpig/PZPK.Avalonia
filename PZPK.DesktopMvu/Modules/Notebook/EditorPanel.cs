using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using PZPK.Core.Note;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using TextMateSharp.Grammars;

namespace PZPK.Desktop.Modules.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class EditorPanel : ComponentBase
{
    private TextEditor BuildEditor()
    {
        RegOptions = new RegistryOptions(ThemeName.DarkPlus);
        SelectedLanguage = RegOptions.GetLanguageByExtension(".md");

        Editor = new TextEditor();
        Editor.FontSize = EditFontSize;
        Editor.Background = Brushes.Transparent;
        Editor.ShowLineNumbers = true;
        Editor.FlowDirection = FlowDirection.LeftToRight;
        Editor.Resources.Add("TextAreaSelectionBrush", Brushes.DarkBlue);
        // Editor.Options.HighlightCurrentLine = true;

        return Editor;
    }
    protected override object Build()
    {
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
                                SukiButton("Save").Margin(10, 0).OnClick(_ => SaveCurrentNote()),
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
    private Note? Current { get; set; }
    private Language SelectedLanguage { get; set; }
    private string ThemeText { get; set; } = Enum.GetName<ThemeName>(ThemeName.DarkPlus) ?? "DarkPlus";
    private Dictionary<string, ThemeName> Themes = new();
    private int EditFontSize = 14;

    string Title { get; set; } = "";
    string Content { get; set; } = "";

    public event Action<Note>? NoteSaved;
    public event Action<Note>? NoteDeleted;

    public void BindNote(Note? note)
    {
        Current = note;
        Title = note?.Title ?? "";
        Content = note?.Content ?? "";

        Editor.Text = Content;

        StateHasChanged();
    }
    private void SaveCurrentNote()
    {
        if (Current is null) return;

        Content = Editor.Text;

        Current.Save(Title, Content);
        NoteSaved?.Invoke(Current);
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
    public void OnDelete()
    {
        App.Instance?.DialogManager
            .CreateDialog()
            .WithActionButton("Delete", e => {
                if (Current is null) return;
                NoteDeleted?.Invoke(Current);
                e.Dismiss();
            })
            .WithActionButton("Cancel ", _ => { }, true)  // last parameter optional
            .TryShow();
    }
}
