using Avalonia.Controls;
using Avalonia.Markup.Declarative;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

internal class NameDialogContent : ContentControl
{
    private TextBox _textbox;
    public NameDialogContent(string originalName)
    {
        _textbox = new TextBox().Text(originalName);
        Content = VStackPanel(Avalonia.Layout.HorizontalAlignment.Stretch)
            .Children(
                PzText("Name:").Margin(0,0,0,15),
                _textbox.HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
            );
    }

    public string? GetResult()
    {
        return _textbox.Text;
    }
}
