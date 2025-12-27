using Avalonia;
using Avalonia.Layout;
using PZPK.Core;
using System.Text;

namespace PZPK.Desktop.Previews.TextPreview;

internal class TextPreviewWindow : PZWindowBase
{
    private PZFile? _file;
    private string _encoding = "Default";
    private TextBox TextView { get; init; }

    public TextPreviewWindow() : base()
    {
        string[] encodings = ["Default", "UTF8", "ASCII"];
        int[] fontsizes = [10, 12, 14, 16, 18, 20, 24, 28, 32, 36, 40, 48, 56, 64, 72];
        TextView = new TextBox();

        Content = Grid(null, "auto, *")
            .Children(
                HStackPanel()
                    .Row(0)
                    .Spacing(10)
                    .Margin(10)
                    .Children(
                        PzText("Encoding").VerticalAlignment(VerticalAlignment.Center),
                        new ComboBox()
                            .ItemsSource(encodings)
                            .SelectedItem("Default")
                            .OnSelectionChanged(EncodingChanged),
                        PzText("FontSize").VerticalAlignment(VerticalAlignment.Center),
                        new ComboBox()
                            .ItemsSource(fontsizes)
                            .SelectedItem(14)
                            .OnSelectionChanged(FontSizeChanged)
                    ),
                TextView
                    .Row(1)
                    .IsReadOnly(true)
                    .Margin(10)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .VerticalAlignment(VerticalAlignment.Stretch)
                    .TextAlignment(Avalonia.Media.TextAlignment.Start)
                    .VerticalContentAlignment(VerticalAlignment.Top)
                    .FontSize(14)
            );

#if DEBUG
        this.AttachDevTools();
#endif
    }

    public void OpenFile(PZFile file)
    {
        _file = file;
        LoadText();
        Title = _file.Name;
    }
    private void LoadText()
    {
        PackageManager.Check();
        if (_file is null) return;

        Span<byte> bytes = new byte[_file.OriginSize];
        PackageManager.Current.ExtractFile(_file, bytes);
        TextView.Text = GetEncoding().GetString(bytes);
    }

    private Encoding GetEncoding()
    {
        return _encoding switch
        {
            "UTF8" => Encoding.UTF8,
            "ASCII" => Encoding.ASCII,
            _ => Encoding.Default,
        };
    }
    private void EncodingChanged(SelectionChangedEventArgs e)
    {
        e.Handled = true;
        if (e.Source is ComboBox cb && cb.SelectedItem is string s)
        {
            _encoding = s;
            LoadText();
        }
    }
    private void FontSizeChanged(SelectionChangedEventArgs e)
    {
        e.Handled = true;
        if (e.Source is ComboBox cb && cb.SelectedItem is int s)
        {
            TextView.FontSize = s;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = true;
        base.OnClosing(e);

        this.Hide();
    }
}
