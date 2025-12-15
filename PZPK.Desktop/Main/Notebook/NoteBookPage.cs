namespace PZPK.Desktop.Main.Notebook;

using System.Reactive.Linq;
using static PZPK.Desktop.Common.ControlHelpers;

public class NoteBookPage : PZComponentBase
{
    protected override Control Build()
    {
        return new Panel()
            .Children(
                new OpenFilePanel()
                    .IsVisible(Model.Notebook.Select(n => n is null)),
                Grid("200,*")
                    .IsVisible(Model.Notebook.Select(n => n is not null))
                    .Children(
                        new NoteListPanel().Col(0),
                        new EditorPanel().Col(1)
                    )
            );
    }

    private NoteBookModel Model { get; init; } = NoteBookModel.Instance;
}
