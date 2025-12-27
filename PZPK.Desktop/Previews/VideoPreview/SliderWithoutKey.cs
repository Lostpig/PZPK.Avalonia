using Avalonia.Input;

namespace PZPK.Desktop.Previews.VideoPreview;

internal class SliderWithoutKey : Slider
{
    public SliderWithoutKey() : base() { }
    protected override void OnKeyDown(KeyEventArgs e)
    {
        // base.OnKeyDown(e);
        e.Handled = false;
    }
}
