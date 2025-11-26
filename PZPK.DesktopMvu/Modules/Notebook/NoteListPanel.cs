using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using Material.Icons;
using PZPK.Core.Note;
using PZPK.Desktop.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PZPK.Desktop.Modules.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;
using static System.Runtime.InteropServices.JavaScript.JSType;
using PZNotebook = PZPK.Core.Note.NoteBook;

public class NoteListItem : ContentControl, ISelectable
{
    private NoteListPanel _parent;
    private TextBlock _text;
    private bool _isOver = false;

    public Note Data { get; init; }
    public bool IsSelected { 
        get; 
        set
        {
            field = value;
            UpdateBg();
        } 
    } = false;
    public IBrush hoverBg = App.Instance.Suki.GetSukiColor("SukiPrimaryColor25");
    public IBrush selectedBg = App.Instance.Suki.GetSukiColor("SukiPrimaryColor50");

    public NoteListItem(NoteListPanel parent, Note data)
    {
        _parent = parent;
        Data = data;

        _text = TextedBlock(Data.Title).VerticalAlignment(VerticalAlignment.Center);
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

        if (IsSelected) return;
        IsSelected = true;
        _parent.UpdateSelected(this);
    }
}

public class NoteListPanel : ComponentBase
{
    private NoteListItem CreateItem(Note note)
    {
        var item = new NoteListItem(this, note);
        Items.Add(item);
        return item;
    }

    protected override object Build()
    {
        var borderColor = App.Instance.Suki.GetSukiColor("SukiBorderBrush");
        return new Border()
            .BorderThickness(0, 0, 1, 0)
            .BorderBrush(borderColor)
            .Child(
                Grid(null, "60, 1*").Children(
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
                            VStackPanel(HorizontalAlignment.Stretch).Classes("list").Ref(out ListContainer)
                        )
                )
            );
    }

    private PZNotebook? Notebook;
    private StackPanel ListContainer;
    private List<NoteListItem> Items = [];

    public event Action<Note?>? NoteSelected;
    public event Action? NoteBookClosed;

    public void BindNotebook(PZNotebook? notebook)
    {
        Notebook = notebook;

        if (notebook != null)
        {
            Items.Clear();
            ListContainer.Children.Clear();

            foreach (Note note in notebook.Notes)
            {
                ListContainer.Children.Add(CreateItem(note));
            }
            if (Items.Count > 0)
            {
                Items[0].IsSelected = true;
            }
        }
    }
    public void AddNote()
    {
        if (Notebook is null) return;

        var newNote = Notebook.AddNote();
        ListContainer.Children.Add(CreateItem(newNote));
    }
    public void UpdateItem(Note note)
    {
        foreach (var item in Items)
        {
            if (item.Data == note)
            {
                item.UpdateTitle();
            }
        }
    }
    public void UpdateSelected(NoteListItem item)
    {
        foreach (var n in Items)
        {
            if (n.IsSelected && n != item) n.IsSelected = false;
        }

        NoteSelected?.Invoke(item.Data);
    }

    public void SaveNoteBook()
    {
        Notebook?.Save();
    }
    public void CloseNoteBook()
    {
        Items.Clear();
        ListContainer.Children.Clear();

        NoteBookClosed?.Invoke();
        StateHasChanged();
    }
}
