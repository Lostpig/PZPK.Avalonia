

namespace PZPK.Desktop.Modules.Notebook;
using PZNotebook = PZPK.Core.Note.NoteBook;

internal class NoteBookModel
{
    static public bool HasOpened => Current != null;
    public static NoteBookModel? Current { get; private set; }
    static public void Open(string file, string password)
    {
        var notebook = PZNotebook.Open(file, password);
        Current = new NoteBookModel(notebook);
    }
    static public void Create(string file, string password)
    {
        var notebook = PZNotebook.Create(file, password);
        Current = new NoteBookModel(notebook);
    }

    public PZNotebook Notebook { get; init; }
    private NoteBookModel(PZNotebook notebook)
    {
        Notebook = notebook;
    }
    public void Close()
    {
        Notebook.Dispose();
        Current = null;
    }
}
