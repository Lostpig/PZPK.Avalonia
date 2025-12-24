using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Material.Icons;

namespace PZPK.Desktop.Main.Notebook;

using static PZPK.Desktop.Common.ControlHelpers;

public class NoteListPanel : PZComponentBase
{
    private class NoteListItem : ContentControl
    {
        private readonly IDisposable _subscription;
        public RxNote Note { get; private set; }
        public NoteListItem(RxNote note)
        {
            Note = note;

            var text = new TextBlock().VerticalAlignment(VerticalAlignment.Center);
            _subscription = Note.Title.Subscribe(s => text.Text = s);

            Content = text;
        }

        protected override void OnUnloaded(RoutedEventArgs e)
        {
            base.OnUnloaded(e);
            _subscription?.Dispose();
        }
    }

    protected override Control Build()
    {
        return new Border()
            .BorderThickness(0, 0, 1, 0)
            .BorderBrush(() => Suki.GetSukiColor("SukiBorderBrush"))
            .Child(
                Grid(null, "60, 1*").Children(
                    HStackPanel(VerticalAlignment.Center)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Row(0)
                        .Children(
                            IconButton(MaterialIconKind.Add)
                                .Margin(10, 0)
                                .ToolTip(() => LOC.Base.Add)
                                .OnClick(_ => Model.NewNote()),
                            IconButton(MaterialIconKind.ContentSave)
                                .Margin(10, 0)
                                .ToolTip(() => LOC.Base.Save)
                                .OnClick(_ => Model.Save()),
                            IconButton(MaterialIconKind.Close)
                                .Margin(10, 0)
                                .ToolTip(() => LOC.Base.Close)
                                .OnClick(_ => Model.Close())
                        ),
                    new ScrollViewer()
                        .Row(1)
                        .Content(
                            new ListBox()
                                .HorizontalAlignment(HorizontalAlignment.Stretch)
                                .ItemsSourceEx(Model.Notes)
                                .SelectedItemEx(Model.Note)
                                .ItemTemplate<RxNote>(n => n is null ? PzText("") : new NoteListItem(n))
                        )
                )
            );
    }

    private static NoteBookModel Model => NoteBookModel.Instance;
}
