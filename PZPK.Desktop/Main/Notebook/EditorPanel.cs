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
    private void InitializeEditor(TextEditor editor, RegistryOptions regOptions, TextMate.Installation textMateInstallation)
    {
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
        var editor = BuildEditor();
        var regOptions = new RegistryOptions(ThemeName.DarkPlus);
        var textMateInstallation = editor.InstallTextMate(regOptions);
        var langs = regOptions.GetAvailableLanguages();

        int[] fontSizes = [12, 14, 16, 18, 20, 24, 28, 32, 40, 48, 56, 64, 72];
        var fonts = FontManager.Current.SystemFonts.OrderBy(f => f.Name);

        var content = Grid(null, "80, 45, 1*").
            Children(
                Grid("*, Auto")
                    .Row(0)
                    .Margin(30, 20)
                    .Children(
                        PzTextBox(Title).Col(0),
                        HStackPanel()
                            .Col(1)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Spacing(10)
                            .Children(
                                SukiButton(() => LOC.Base.Save).OnClick(_ => SaveNote()),
                                SukiButton(() => LOC.Base.Delete, "Accent").OnClick(_ => OnDelete())
                            )
                    ),
                HStackPanel()
                    .Row(1)
                    .Margin(30, 0, 30, 5)
                    .Children(
                        new ComboBox()
                            .ItemsSource(langs)
                            .SelectedItemEx(Language),
                        new ComboBox()
                            .ItemsSource(Enum.GetValues<ThemeName>())
                            .SelectedItemEx(EditorTheme),
                        new ComboBox()
                            .ItemsSource(fonts)
                            .SelectedItemEx(Font),
                        new ComboBox()
                            .ItemsSource(fontSizes)
                            .SelectedItemEx(FontSize)
                    ),
                new SukiUI.Controls.GlassCard()
                    .Row(2)
                    .Margin(30, 0, 30, 20)
                    .Content(
                        editor
                    )
            );

        InitializeEditor(editor, regOptions, textMateInstallation);

        return content;
    }

    private Subject<string> Title { get; init; } = new();
    private Subject<string> Content { get; init; } = new();
    private Subject<Language> Language { get; init; } = new();
    private Subject<FontFamily> Font { get; set; } = new();
    private Subject<int> FontSize { get; set; } = new();
    private Subject<ThemeName> EditorTheme { get; set; } = new();

    private readonly List<IDisposable> _subscriptions = [];

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return [
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
            })
        ];
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

        var opt = PZDialog.ConfirmOptions(LOC.Base.Warning, LOC.Message.SureToDelete);
        var ok = await Model.Dialog.ShowDialog(opt);
        if (ok)
        {
            Model.DeleteNote();
        }
    }
}
