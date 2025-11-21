using Avalonia.Media;

namespace PZPK.Desktop.Modules.ImagePreview;

public enum LockMode
{
    None,
    Scale,
    FitHeight,
    FitWidth
}

public class PreviewModel
{
    public double Scale { get; set; } = 1;
    public string ScalePercent => $"{(int)(Scale * 100)}%";
    public LockMode Lock { get; set; } = LockMode.None;

    public string FileName { get; set; } = "";
    public int Current { get; set; } = 0;
    public int Total { get; set; } = 0;
    public Avalonia.PixelSize OriginSize { get; set; } = new(0, 0);
    public Avalonia.PixelSize RenderedSize { get; set; } = new(0, 0);
    public string SizeText { get; set; } = "0 x 0";
    public string FileSizeText { get; set; } = "0";
}
