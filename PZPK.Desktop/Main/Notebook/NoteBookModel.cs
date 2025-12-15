using PZ.RxAvalonia.Reactive;
using PZPK.Core.Note;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZPK.Desktop.Main.Notebook;
using PZNotebook = PZPK.Core.Note.NoteBook;

public sealed class RxNote : IDisposable
{
    public Note Note { get; init; }
    public NoteBook Book { get; init; }
    private readonly IDisposable _subscription;
    public BehaviorSubject<string> Title { get; init; }
    public BehaviorSubject<string> Content { get; init; }
    public RxNote(Note note, PZNotebook book)
    {
        Note = note;
        Book = book;
        Title = new(note.Title);
        Content = new(note.Content);

        _subscription = Title.CombineLatest(Content)
            .Throttle(TimeSpan.FromSeconds(1))
            .Subscribe(t => Save(t.First, t.Second));
        Book = book;
    }

    public void Save()
    {
        Save(Title.Value, Content.Value);
    }
    private void Save(string title, string content)
    {
        Note.Save(title, content);
    }

    public void Dispose()
    {
        _subscription.Dispose();
    }
}

public class NoteBookModel : PageModelBase
{
    private static NoteBookModel? _instance;
    public static NoteBookModel Instance
    {
        get
        {
            _instance ??= new();
            return _instance;
        }
    }

    public BehaviorSubject<PZNotebook?> Notebook { get; init; } = new(null);
    public BehaviorSubject<RxNote?> Note { get; init; } = new(null);
    public ReactiveList<RxNote> Notes { get; init; } = [];

    private NoteBookModel() 
    {
        Notebook.Subscribe(book =>
        {
            Notes.Clear();
            if (book != null)
            {
                Notes.AddRange(book.Notes.Select(n => new RxNote(n, book)));
            }
        });

        Notes.WhenAdd.Subscribe(ns =>
        {
            Note.OnNext(ns.FirstOrDefault());
        });
        Notes.WhenRemove.Subscribe(removed =>
        {
            foreach (var n in removed)
            {
                n.Book.DeleteNote(n.Note);
                n.Dispose();
            }

            Note.OnNext(Notes.Count > 0 ? Notes[0] : null);
        });
    }

    public void NewNote()
    {
        if (Notebook.Value == null)
        {
            Toast.Error("Notebook not opened!"); 
            return;
        }

        var newNote = Notebook.Value.AddNote();
        Notes.Add(new RxNote(newNote, Notebook.Value));
    }
    public void Open(string path, string password)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        try
        {
            Notebook.OnNext(PZNotebook.Open(path, password));
        }
        catch (Exception ex)
        {
            Toast.Error(ex.Message);
            Logger.Instance.Log(ex.Message);
        }
    }
    public void Create(string path, string password, string repeatPassword)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }
        if (password != repeatPassword)
        {
            Toast.Error("Password not match");
            return;
        }

        try
        {
            Notebook.OnNext(PZNotebook.Create(path, password));
        }
        catch (Exception ex)
        {
            Toast.Error(ex.Message);
            Logger.Instance.Log(ex.Message);
        }
    }
    public void Save()
    {
        Notebook.Value?.Save();
    }
    public void Close()
    {
        Notebook.Value?.Dispose();
        Notebook.OnNext(null);
    }
}
