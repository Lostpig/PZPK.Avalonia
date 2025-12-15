using Avalonia.Layout;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.TextMate;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TextMateSharp.Grammars;

namespace PZPK.Desktop.Main.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class EditorPanel: PZComponentBase
{
    private static NoteBookModel Model => NoteBookModel.Instance;

    private static TextEditor BuildEditor()
    {
        var editor = new TextEditor()
        {
            Background = Brushes.Transparent,
            ShowLineNumbers = true
        };
        editor.Options.ShowSpaces = true;
        editor.FlowDirection = FlowDirection.LeftToRight;
        editor.Resources.Add("TextAreaSelectionBrush", Brushes.DarkBlue);

        return editor;
    }
    private void InitializeEditor(TextEditor editor, RegistryOptions regOptions)
    {
        var textMateInstallation = editor.InstallTextMate(regOptions);

        Language.Subscribe(l =>
        {
            var sn = regOptions.GetScopeByLanguageId(l.Id);
            textMateInstallation.SetGrammar(sn);
        });
        EditorTheme.Subscribe(t =>
        {
            var rt = regOptions.LoadTheme(t);
            textMateInstallation.SetTheme(rt);
        });
        Font.Subscribe(f => editor.FontFamily = f);
        FontSize.Subscribe(s => editor.FontSize = s);

        var defaultLang = regOptions.GetLanguageByExtension(".md");
        Language.OnNext(defaultLang);
        EditorTheme.OnNext(ThemeName.DarkPlus);
        Font.OnNext(FontFamily.Parse("Consolas"));
        FontSize.OnNext(14);

        Observable.FromEvent<EventHandler, EventArgs>(
            h => (s,e) => h(e),
            h => editor.TextChanged += h,
            h => editor.TextChanged -= h
        ).Subscribe(_ => Content.OnNext(editor.Text));

        Content.Subscribe(t =>
        {
            if (t != editor.Text) editor.Text = t;
        });
    }
    protected override Control Build()
    {
        var regOptions = new RegistryOptions(ThemeName.DarkPlus);
        var editor =  BuildEditor();

        int[] fontSizes = [12, 14, 16, 18, 20, 24, 28, 32, 40, 48, 56, 64, 72];
        var fonts = FontManager.Current.SystemFonts.OrderBy(f => f.Name);

        var content = Grid(null, "80, 45, 1*").
            Children(
                Grid("*, 200")
                    .Row(0)
                    .Margin(30, 20)
                    .Children(
                        PzTextBox(Title).Col(0),
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
                            .ItemsSource(regOptions.GetAvailableLanguages())
                            .ItemTemplate<Language>(l => PzText(l.Id))
                            .SelectedItem(Language),
                        new ComboBox()
                            .ItemsSource(Enum.GetValues<ThemeName>())
                            .ItemTemplate<ThemeName>(t => PzText(t.ToString()))
                            .SelectedItem(EditorTheme),
                        new ComboBox()
                            .ItemsSource(fonts)
                            .ItemTemplate<FontFamily>(f => PzText(f.Name))
                            .SelectedItem(Font),
                        new ComboBox()
                            .ItemsSource(fontSizes)
                            .SelectedItem(FontSize)
                    ),
                new SukiUI.Controls.GlassCard()
                    .Row(2)
                    .Margin(30, 0, 30, 20)
                    .Content(
                        editor
                    )
            );

        InitializeEditor(editor, regOptions);

        return content;
    }

    private Subject<string> Title { get; init; } = new();
    private Subject<string> Content { get; init; } = new();
    private Subject<Language> Language { get; init; } = new();
    private Subject<FontFamily> Font { get; set; } = new();
    private Subject<int> FontSize { get; set; } = new();
    private Subject<ThemeName> EditorTheme { get; set; } = new();

    private readonly List<IDisposable> _subscriptions = [];
    protected override void OnCreated()
    {
        base.OnCreated();
        Model.Note.Subscribe(n =>
        {
            ClearSubscriptions();
            if (n != null)
            {
                Title.OnNext(n.Note.Title);
                Content.OnNext(n.Note.Content);

                _subscriptions.AddRange(
                    Title.Subscribe(n.Title),
                    Content.Subscribe(n.Content)
                );
            }
        });
    }
    private void ClearSubscriptions()
    {
        foreach(var s in _subscriptions)
        {
            s.Dispose();
        }
        _subscriptions.Clear();
    }
    private static void SaveNote()
    {
        Model.Note.Value?.Save();
    }
    private static async void OnDelete()
    {
        if (Model.Note.Value == null) return;

        var ok = await Model.Dialog.DeleteConfirm("Sure to delete?");
        if (ok)
        {
            Model.Notes.Remove(Model.Note.Value);
        }
    }
}
