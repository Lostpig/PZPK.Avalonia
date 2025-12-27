using PZPK.Core;

namespace PZPK.Desktop.Common;

enum PZItemType
{
    Folder,
    Picture,
    Video,
    Audio,
    Text,
    Other
}

internal class PZItemTypeHelper
{
    public static PZItemType GetItemType(string ext)
    {
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".webp"
                => PZItemType.Picture,
            ".mp4" or ".avi" or ".mkv" or ".wmv"
                => PZItemType.Video,
            ".mp3" or ".ogg" or ".flac" or ".ape"
                => PZItemType.Audio,
            ".txt" or ".md" => PZItemType.Text,
            _ => PZItemType.Other
        };
    }
    public static PZItemType GetItemType(IPZFile file)
    {
        return GetItemType(file.Extension);
    }
    public static PZItemType GetItemType(IPZItem item)
    {
        if (item is IPZFolder) return PZItemType.Folder;
        else if (item is IPZFile file) return GetItemType(file);
        else return PZItemType.Other;
    }

    public static bool IsPicture(IPZFile file)
    {
        return GetItemType(file) == PZItemType.Picture;
    }
    public static bool IsVideo(IPZFile file)
    {
        return GetItemType(file) == PZItemType.Video;
    }
    public static bool IsAudio(IPZFile file)
    {
        return GetItemType(file) == PZItemType.Audio;
    }
    public static bool IsText(IPZFile file)
    {
        return GetItemType(file) == PZItemType.Text;
    }
}
