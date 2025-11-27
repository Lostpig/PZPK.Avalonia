using Avalonia.Controls;
using Avalonia.Markup.Declarative;

namespace PZPK.Desktop.Modules.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class NoteBookPage : ComponentBase
{
    protected override object Build()
    {
        var list = new NoteListPanel().Col(0);
        var editor = new EditorPanel().Col(1);

        list.OnNoteSelected(n => editor.BindNote(n))
            .OnNoteBookClosed(OnNoteBookClosed);
        editor.OnNoteSaved(n => list.UpdateItem(n))
              .OnNoteDeleted(n => list.DeleteItem(n));

        return new Panel()
            .Children(
                new OpenFilePanel()
                    .IsVisible(() => Model is null)
                    .OnNotebookOpened(() =>
                    {
                        list.BindNotebook(Model?.Notebook);
                    }),
                Grid("200,*")
                    .IsVisible(() => Model is not null)
                    .Children(
                        list,
                        editor
                    )
            );
    }

    private NoteBookModel? Model => NoteBookModel.Current;

    private void OnNoteBookClosed()
    {
        Model?.Close();
        StateHasChanged();
    }
}
