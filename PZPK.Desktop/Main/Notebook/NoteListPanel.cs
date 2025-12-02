using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using Material.Icons;
using PZPK.Core.Note;
using PZPK.Desktop.Common;

namespace PZPK.Desktop.Modules.Notebook;

using static PZPK.Desktop.Common.ControlHelpers;

public class NoteListPanel : Border
{
    private class NoteListItem : ContentControl, ISelectable
    {
        private NoteListPanel _parent;
        private TextBlock _text;
        private bool _isOver = false;

        public Note Data { get; init; }
        public bool IsSelected
        {
            get;
            set
            {
                if (field == value) return;

                field = value;
                UpdateBg();
                if (field == true) _parent.UpdateSelected(this);
            }
        } = false;
        public IBrush hoverBg = App.Instance.Suki.GetSukiColor("SukiPrimaryColor25");
        public IBrush selectedBg = App.Instance.Suki.GetSukiColor("SukiPrimaryColor50");

        public NoteListItem(NoteListPanel parent, Note data)
        {
            _parent = parent;
            Data = data;

            _text = PzText(Data.Title).VerticalAlignment(VerticalAlignment.Center);
            Content = new Border()
                .Background(Brushes.Transparent)
                .Padding(10, 8)
                .Child(_text);
        }
        public void UpdateTitle()
        {
            _text.Text = Data.Title;
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
            IsSelected = true;
        }
    }

    public NoteListPanel(NoteBookModel vm)
    {
        ViewModel = vm;

        var borderColor = App.Instance.Suki.GetSukiColor("SukiBorderBrush");
        BorderThickness = new Avalonia.Thickness(0, 0, 1, 0);
        BorderBrush = borderColor;

        ListContainer = VStackPanel(HorizontalAlignment.Stretch);
        Child = Grid(null, "60, 1*").Children(
                    HStackPanel(VerticalAlignment.Center)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Row(0)
                        .Children(
                            IconButton(MaterialIconKind.Add).Margin(10, 0).ToolTip("Add").OnClick(_ => AddNote()),
                            IconButton(MaterialIconKind.ContentSave).Margin(10, 0).ToolTip("Save").OnClick(_ => SaveNoteBook()),
                            IconButton(MaterialIconKind.Close).Margin(10, 0).ToolTip("Close").OnClick(_ => CloseNoteBook())
                        ),
                    new ScrollViewer()
                        .Row(1)
                        .Content(
                            ListContainer
                        )
                );

        InitializeModel();
    }

    private NoteBookModel ViewModel { get; init; }
    private StackPanel ListContainer { get; init; }
    private PzControls<NoteListItem> Items => new(ListContainer.Children);

    private void InitializeModel()
    {
        ViewModel?.NoteBookChanged += NoteBookChanged;
        ViewModel?.NoteModified += UpdateItem;
        ViewModel?.NoteDeleted += DeleteItem;
    }

    private void NoteBookChanged()
    {
        var notebook = ViewModel?.Notebook;

        if (notebook != null)
        {
            Items.Clear();

            foreach (Note note in notebook.Notes)
            {
                var item = new NoteListItem(this, note);
                Items.Add(item);
            }
            if (ListContainer.Children.Count > 0)
            {
                Items[0].IsSelected = true;
            }
            else
            {
                ViewModel?.SelectNote(null);
            }
        }
    }
    private void AddNote()
    {
        var notebook = ViewModel?.Notebook;
        if (notebook is null) return;

        var newNote = notebook.AddNote();

        var item = new NoteListItem(this, newNote);
        Items.Add(item);

        item.IsSelected = true;
    }
    private void UpdateItem(Note note)
    {
        foreach (var item in Items)
        {
            if (item.Data == note)
            {
                item.UpdateTitle();
            }
        }
    }
    private void UpdateSelected(NoteListItem item)
    {
        foreach (var n in Items)
        {
            if (n.IsSelected && n != item) n.IsSelected = false;
        }

        ViewModel?.SelectNote(item.Data);
    }
    private void DeleteItem(int id)
    {
        NoteListItem? item = null;
        foreach (var i in Items)
        {
            if (i.Data.Id == id)
            {
                item = i; break;
            }
        }

        if (item != null) {
            Items.Remove(item);
            ListContainer.Children.Remove(item);

            if (Items.Count > 0) 
            {
                Items[0].IsSelected = true;
            }
            else
            {
                ViewModel?.SelectNote(null);
            }
        }
    }

    private void SaveNoteBook()
    {
        ViewModel?.Save();
    }
    private void CloseNoteBook()
    {
        ViewModel?.Close();
    }
}
