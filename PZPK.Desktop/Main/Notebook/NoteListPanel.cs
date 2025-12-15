using Avalonia;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Material.Icons;
using System.Collections.Concurrent;

namespace PZPK.Desktop.Main.Notebook;

using static PZPK.Desktop.Common.ControlHelpers;

public class NoteListPanel : Border
{
    private class NoteListItem : ContentControl, ISelectable
    {
        private static readonly ConcurrentStack<NoteListItem> Pool = new();
        public static NoteListItem GetItem(RxNote note)
        {
            var noteItem = Pool.TryPop(out var item) ? item : new NoteListItem();
            noteItem.BindingData(note);
            return noteItem;
        }

        private readonly TextBlock _text;
        private bool _isOver = false;
        private IDisposable? _subscription;

        public RxNote? Data { get; private set; }
        public bool IsSelected
        {
            get;
            set
            {
                if (field == value) return;

                field = value;
                UpdateBg();
            }
        } = false;
        public IBrush hoverBg = App.Instance.Suki.GetSukiColor("SukiPrimaryColor25");
        public IBrush selectedBg = App.Instance.Suki.GetSukiColor("SukiPrimaryColor50");

        private NoteListItem()
        {
            _text = PzText("").VerticalAlignment(VerticalAlignment.Center);

            Content = new Border()
                .Background(Brushes.Transparent)
                .Padding(10, 8)
                .Child(_text);

            Model.Note.Subscribe(n => this.IsSelected = n == this.Data);
        }
        public void BindingData(RxNote data)
        {
            Data = data;
            _subscription = data.Title.Subscribe(t => _text.Text = t);
        }

        private void UpdateBg()
        {
            if (IsSelected)
            {
                Background = selectedBg;
            }
            else if (_isOver)
            {
                Background = hoverBg;
            }
            else
            {
                Background = null;
            }
        }
        protected override void OnPointerEntered(PointerEventArgs e)
        {
            base.OnPointerEntered(e);
            _isOver = true;
            UpdateBg();
        }
        protected override void OnPointerExited(PointerEventArgs e)
        {
            base.OnPointerExited(e);
            _isOver = false;
            UpdateBg();
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            Model.Note.OnNext(Data);
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _subscription?.Dispose();

            _subscription = null;
            Data = null;

            Pool.Push(this);
        }
    }

    public NoteListPanel()
    {
        var borderColor = App.Instance.Suki.GetSukiColor("SukiBorderBrush");
        BorderThickness = new Avalonia.Thickness(0, 0, 1, 0);
        BorderBrush = borderColor;

        Child = Grid(null, "60, 1*").Children(
                    HStackPanel(VerticalAlignment.Center)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Row(0)
                        .Children(
                            IconButton(MaterialIconKind.Add).Margin(10, 0).ToolTip("Add").OnClick(_ => Model.NewNote()),
                            IconButton(MaterialIconKind.ContentSave).Margin(10, 0).ToolTip("Save").OnClick(_ => Model.Save()),
                            IconButton(MaterialIconKind.Close).Margin(10, 0).ToolTip("Close").OnClick(_ => Model.Close())
                        ),
                    new ScrollViewer()
                        .Row(1)
                        .Content(
                            new ItemsControl()
                                .ItemsPanel(VStackPanel(HorizontalAlignment.Stretch))
                                .ItemsSourceEx(Model.Notes)
                                .ItemTemplate<RxNote, ItemsControl>(n => NoteListItem.GetItem(n))
                        )
                );
    }

    private static NoteBookModel Model => NoteBookModel.Instance;
}
