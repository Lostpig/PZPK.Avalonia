using Avalonia.Controls;
using Avalonia.Markup.Declarative;

namespace PZPK.Desktop.Main.Notebook;
using static PZPK.Desktop.Common.ControlHelpers;

public class NoteBookPage : ComponentBase
{
    protected override object Build()
    {
        InitializeModel();

        return new Panel()
            .Children(
                new OpenFilePanel(Model)
                    .IsVisible(() => Model.Notebook is null),
                Grid("200,*")
                    .IsVisible(() => Model.Notebook is not null)
                    .Children(
                        new NoteListPanel(Model).Col(0),
                        new EditorPanel(Model).Col(1)
                    )
            );
    }

    private NoteBookModel Model { get; init; } = new();
    private void InitializeModel()
    {
        // Model.NoteChanged += StateHasChanged;
        Model.NoteBookChanged += StateHasChanged;
    }
}
