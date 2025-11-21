using System.Xml.Linq;

namespace PZPK.Core.Exceptions;

public class OldVersionEncryptException(string? message = default) : Exception(message);

public class SourceDirectoryIsEmptyException(string dirPath, string? message = default) : Exception(message)
{
    public string DirectoryPath { get; init; } = dirPath;
}

public class OutputDirectoryIsNotEmptyException(string dirPath, string? message = default) : Exception(message)
{
    public string DirectoryPath { get; init; } = dirPath;
}

public class OutputFileAlreadyExistsException(string fileName, string? message = default) : Exception(message)
{
    public string FileName { get; init; } = fileName;
}

public class FileVersionNotCompatiblityException(int version, string? message = default) : Exception(message)
{
    public int Version { get; init; } = version;
}

public class PZSignCheckedException(string? message = default) : Exception(message);

public class PZPasswordIncorrectException(string? message = default) : Exception(message);

public class PathIsNotDirectoryException(string dirPath, string? message = default) : Exception(message)
{
    public string DirectoryPath { get; init; } = dirPath;
}

public class PZFolderNotFoundException(string name, int id, string? message = default) : Exception(message)
{
    public int Id { get; init; } = id;
    public string Name { get; init; } = name;
}

public class PZFileNotFoundException(string name, int id, string? message = default) : Exception(message)
{
    public int Id { get; init; } = id;
    public string Name { get; init; } = name;
}

public class FileInIndexNotEncodeException(string name, int id, string? message = default) : Exception(message)
{
    public int Id { get; init; } = id;
    public string Name { get; init; } = name;
}

public class DuplicateNameException(string name, string? message = default) : Exception(message)
{
    public string Name { get; init; } = name;
}

public class CreatorInvaildException(string? message = default) : Exception(message);

public class FileTypeMismatchException(string fileType, string package, string? message = default) : Exception(message)
{
    public string FileType { get; init; } = fileType;
    public string Package { get; init; } = package;
}

public class PZNoteSizeExceededException(int maxSize, int actualSize, string? message = default) : Exception(message)
{
    public int MaxSize { get; init; } = maxSize;
    public int ActualSize { get; init; } = actualSize;
}