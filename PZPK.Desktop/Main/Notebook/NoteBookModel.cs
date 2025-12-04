using PZPK.Core.Note;
using PZPK.Desktop.Main;
using System;

namespace PZPK.Desktop.Main.Notebook;
using PZNotebook = PZPK.Core.Note.NoteBook;

public class NoteBookModel
{
    public PZNotebook? Notebook { get; private set; }
    public Note? Note { get; private set; }

    public Action? NoteChanged;
    public Action? NoteBookChanged;
    public Action<int>? NoteDeleted;
    public Action<Note>? NoteModified;

    public void SelectNote(Note? note)
    {
        Note = note;
        NoteChanged?.Invoke();
    }
    public void DeleteNote()
    {
        if(Note is null) return;

        Notebook?.DeleteNote(Note);
        NoteDeleted?.Invoke(Note.Id);
    }
    public void ModifyNote(string title, string content)
    {
        if (Note is null) return;

        Note.Save(title, content);
        NoteModified?.Invoke(Note);
    }
    public void Open(string path, string password)
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        try
        {
            Notebook = PZNotebook.Open(path, password);
            NoteBookChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Toast.Error(ex.Message);
            Global.Logger.Instance.Log(ex.Message);
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
            Notebook = PZNotebook.Create(path, password);
            NoteBookChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Toast.Error(ex.Message);
            Global.Logger.Instance.Log(ex.Message);
        }
    }

    public void Save()
    {
        Notebook?.Save();
    }
    public void Close()
    {
        Notebook?.Dispose();
        Notebook = null;
        Note = null;

        NoteBookChanged?.Invoke();
        NoteChanged?.Invoke();
    }
}
