using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using PZPK.Core.Note;
using PZPK.Desktop.Common;
using System;
using System.Collections.Generic;

namespace PZPK.Desktop.Modules.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;
using PZNotebook = PZPK.Core.Note.NoteBook;

public class NoteListPanel : ComponentBase
{
    Dictionary<int, Border> Items = [];
    private Border CreateItem(Note note)
    {
        var item = new Border().Child(
                TextedBlock(note.Title).Padding(10)
            ).OnPointerPressed(_ => SelectItem(note));
        Items.Add(note.Id, item);

        return item;
    }
    private void RenderItems(List<Note> notes)
    {
        Items.Clear();
        ListContainer.Children.Clear();

        foreach (Note note in notes)
        {
            ListContainer.Children.Add(CreateItem(note));
        }
    }
    private void SelectItem(Note note)
    {
        Items.TryGetValue(note.Id, out var item);
        if (item != null)
        {
            if (SelectedItem != null)
            {
                Items.TryGetValue(SelectedItem.Id, out var oldItem);
                oldItem?.Background = null;
            }

            SelectedItem = note;
            item.Background = App.Instance.Suki.GetSukiColor("SukiPrimaryColor50");

            NoteSelected?.Invoke(note);
        }
    }

    protected override object Build()
    {
        return 
            new Border().BorderThickness(0,0,1,0).Child(
                Grid(null, "100, 1*").Children(
                    VStackPanel(HorizontalAlignment.Center)
                        .Row(0)
                        .Children(
                            PzButton("Add+").OnClick(_ => AddNote()),
                            PzButton("Save Book").OnClick(_ => SaveNoteBook()),
                            PzButton("Close").OnClick(_ => CloseNoteBook())
                        ),
                    new ScrollViewer()
                        .Row(1)
                        .Content(
                            VStackPanel().Ref(out ListContainer)
                        )
                )
            );
    }

    private PZNotebook? Notebook;
    private StackPanel ListContainer;
    private Note? SelectedItem;

    public event Action<Note?>? NoteSelected;
    public event Action? NoteBookClosed;

    public void BindNotebook(PZNotebook? notebook)
    {
        Notebook = notebook;

        if (notebook != null)
        {
            RenderItems(notebook.Notes);
        }
    }
    public void AddNote()
    {
        if (Notebook is null) return;

        var newNote = Notebook.AddNote();
        ListContainer.Children.Add(CreateItem(newNote));
        SelectItem(newNote);
    }
    public void UpdateItem(Note note)
    {
        Items.TryGetValue(note.Id, out var item);
        if (item != null && item.Child is TextBlock tb)
        {
            tb.Text = note.Title;
        }
    }

    public void SaveNoteBook()
    {
        Notebook?.Save();
    }
    public void CloseNoteBook()
    {
        Items.Clear();

        NoteBookClosed?.Invoke();
        StateHasChanged();
    }
}
