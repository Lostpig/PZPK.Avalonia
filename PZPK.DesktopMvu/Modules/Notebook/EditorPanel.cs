using Avalonia.Markup.Declarative;
using PZPK.Core.Note;
using System;

namespace PZPK.Desktop.Modules.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class EditorPanel : ComponentBase
{
    protected override object Build()
    {
        return Grid(null, "100, 1*").
            Children(
                HStackPanel().Row(0)
                    .Margin(10)
                    .Children(
                        PzTextBox(() => Title, v => Title = v).Row(0).Width(200),
                        PzButton("Save").OnClick(_ => SaveCurrentNote()),
                        PzButton("Delete").OnClick(_ => SaveCurrentNote())
                    ),
                PzTextBox(() => Content, v => Content = v)
                    .Margin(30)
                    .Row(1)
                    .AcceptsReturn(true)
            );
    }

    private Note? Current { get; set; }
    string Title { get; set; } = "";
    string Content { get; set; } = "";

    public event Action<Note>? NoteSaved;

    public void BindNote(Note? note)
    {
        Current = note;
        Title = note?.Title ?? "";
        Content = note?.Content ?? "";

        StateHasChanged();
    }
    private void SaveCurrentNote()
    {
        if (Current is null) return;

        Current.Save(Title, Content);
        NoteSaved?.Invoke(Current);
    }
}
