using Avalonia.Controls;
using Avalonia.Markup.Declarative;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

internal class NameDialogContent : ContentControl
{
    public string Result { get; private set; } = "";
    public NameDialogContent()
    {
        var input = new TextBox();
        input.TextChanged += (s, e) =>
        {
            Result = input.Text ?? "";
        };

        Content = Grid(null, "*, *")
            .Children(
                PzText("Name:").Row(0),
                input
            );
    }
}
