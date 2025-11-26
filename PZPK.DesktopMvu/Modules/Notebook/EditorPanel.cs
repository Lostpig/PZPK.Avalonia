using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using PZPK.Core.Note;
using System;

namespace PZPK.Desktop.Modules.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class EditorPanel : ComponentBase
{
    protected override object Build()
    {
        Editor = new AvaloniaEdit.TextEditor()
                    .Margin(30, 10, 30, 20)
                    .Row(1);
        Editor.ShowLineNumbers = true;
        Editor.FlowDirection = Avalonia.Media.FlowDirection.LeftToRight;

        return Grid(null, "70, 1*").
            Children(
                Grid("*, 200")
                    .Row(0)
                    .Margin(30, 20, 30, 10)
                    .Children(
                        PzTextBox(() => Title, v => Title = v).Col(0),
                        HStackPanel()
                            .Col(1)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Children(
                                SukiButton("Save").Margin(10, 0).OnClick(_ => SaveCurrentNote()),
                                SukiButton("Delete", "Accent").Margin(10, 0).OnClick(_ => SaveCurrentNote())
                            )
                    ),
                Editor
            );
    }

    private AvaloniaEdit.TextEditor Editor;
    private Note? Current { get; set; }
    string Title { get; set; } = "";
    string Content { get; set; } = "";

    public event Action<Note>? NoteSaved;

    public void BindNote(Note? note)
    {
        Current = note;
        Title = note?.Title ?? "";
        Content = note?.Content ?? "";

        Editor.Text = Content;

        StateHasChanged();
    }
    private void SaveCurrentNote()
    {
        if (Current is null) return;

        Content = Editor.Text;

        Current.Save(Title, Content);
        NoteSaved?.Invoke(Current);
    }
}
