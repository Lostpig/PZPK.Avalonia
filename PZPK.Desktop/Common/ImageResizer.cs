using PZPK.Core;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.IO;


namespace PZPK.Desktop.Common;

public enum ImageResizerFormat
{
    Jpeg,
    Png,
    Webp
}
public record ImageResizerOptions(ImageResizerFormat Format, int Quality, int MaxSize, bool Lossless);
public class ImageResizer : IImageResizer
{
    public static ImageResizer CreateResizer(ImageResizerOptions options)
    {
        return options.Format switch
        {
            ImageResizerFormat.Jpeg => CreateJpegResizer(options.MaxSize, options.Quality),
            ImageResizerFormat.Png => CreatePngResizer(options.MaxSize),
            ImageResizerFormat.Webp => CreateWebpResizer(options.MaxSize, options.Lossless, options.Quality),
            _ => throw new System.NotImplementedException(),
        };
    }
    public static ImageResizer CreateJpegResizer(int maxSize, int quality)
    {
        JpegEncoder encoder = new() { Quality = quality };
        return new ImageResizer(maxSize, encoder, ".jpg");
    }
    public static ImageResizer CreatePngResizer(int maxSize)
    {
        PngEncoder encoder = new();
        return new ImageResizer(maxSize, encoder, ".png");
    }
    public static ImageResizer CreateWebpResizer(int maxSize, bool lossless, int quality)
    {
        WebpFileFormatType type = lossless ? WebpFileFormatType.Lossless : WebpFileFormatType.Lossy;
        WebpEncoder encoder = new() { FileFormat = type, Quality = quality };
        return new ImageResizer(maxSize, encoder, ".webp");
    }

    private readonly ImageEncoder Encoder;
    private readonly int MaxSize;
    public string Extension { get; private set; }

    private ImageResizer(int maxSize, ImageEncoder encoder, string extension)
    {
        MaxSize = maxSize;
        Encoder = encoder;
        Extension = extension;
    }

    private double ComputeResizeScale(int w, int h)
    {
        int large = w > h ? w : h;
        if (large <= MaxSize || MaxSize <= 0) return 1;

        return MaxSize / (double)large;
    }

    public MemoryStream ResizeImage(Stream source)
    {
        using var img = SixLabors.ImageSharp.Image.Load(source);
        var scale = ComputeResizeScale(img.Width, img.Height);

        if (scale < 1)
        {
            int newWidth = (int)(img.Width * scale);
            int newHeight = (int)(img.Height * scale);
            img.Mutate(img => img.Resize(newWidth, newHeight));
        }

        MemoryStream result = new();
        img.Save(result, Encoder);

        return result;
    }

    public bool CheckIsImageFile(IPZFile file)
    {
        return file.Extension == ".jpg" || file.Extension == ".jpeg" || file.Extension == ".png";
    }
}

