using PZPK.Core;

namespace PZPK.Desktop.Common;

enum PZFileType
{
    Folder,
    Picture,
    Video,
    Audio,
    Other
}

internal class FileTypeHelper
{
    public static PZFileType GetFileType(string ext)
    {
        return ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" or ".webp"
                => PZFileType.Picture,
            ".mp4" or ".avi" or ".mkv" or ".wmv"
                => PZFileType.Video,
            ".mp3" or ".ogg" or ".flac" or ".ape"
                => PZFileType.Audio,
            _ => PZFileType.Other
        };
    }
    public static PZFileType GetFileType(PZFile file)
    {
        return GetFileType(file.Extension);
    }

    public static bool IsPicture(PZFile file)
    {
        return GetFileType(file) == PZFileType.Picture;
    }
    public static bool IsVideo(PZFile file)
    {
        return GetFileType(file) == PZFileType.Video;
    }
    public static bool IsAudio(PZFile file)
    {
        return GetFileType(file) == PZFileType.Audio;
    }
}
