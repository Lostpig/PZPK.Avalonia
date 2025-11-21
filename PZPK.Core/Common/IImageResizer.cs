namespace PZPK.Core;

public interface IImageResizer
{
    string Extension { get; }
    public MemoryStream ResizeImage(Stream source);
    public bool CheckIsImageFile(IPZFile file);
}
