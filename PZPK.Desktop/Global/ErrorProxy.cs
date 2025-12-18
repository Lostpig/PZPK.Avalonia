using Ex = PZPK.Core.Exceptions;

namespace PZPK.Desktop.Global;

internal class ErrorProxy
{
    private static string FormatException(Exception ex)
    {
        return ex switch
        {
            Ex.CreatorInvaildException => string.Format(LOC.Error.PZCreatorInvaild, ex.Message),
            Ex.DuplicateNameException ee => string.Format(LOC.Error.PZDuplicateName, ee.Name),
            Ex.EmptyStringException ee => string.Format(LOC.Error.PZEmptyString, ee.Argument),
            Ex.FileTypeMismatchException ee => string.Format(LOC.Error.PZFileTypeMismatch, ee.FileType),
            Ex.FileVersionNotCompatiblityException ee => string.Format(LOC.Error.PZFileVersionNotCompatiblity, ee.Version),
            Ex.OldVersionEncryptException => LOC.Error.PZOldVersionEncrypt,
            Ex.OutputDirectoryIsNotEmptyException ee => string.Format(LOC.Error.PZOutputDirectoryIsNotEmpty, ee.DirectoryPath),
            Ex.OutputFileAlreadyExistsException ee => string.Format(LOC.Error.PZOutputFileAlreadyExists, ee.FileName),
            Ex.PathIsNotDirectoryException ee => string.Format(LOC.Error.PZPathIsNotDirectory, ee.DirectoryPath),
            Ex.PZFileNotFoundException ee => string.Format(LOC.Error.PZFileNotFound, ee.Id, ee.Name),
            Ex.PZFolderNotFoundException ee => string.Format(LOC.Error.PZFolderNotFound, ee.Id, ee.Name),
            Ex.PZNoteSizeExceededException ee => string.Format(LOC.Error.PZNoteSizeExceeded, ee.ActualSize, ee.MaxSize),
            Ex.PZPasswordIncorrectException => LOC.Error.PZPasswordIncorrect,
            Ex.PZSignCheckedException => LOC.Error.PZSignChecked,
            Ex.SourceDirectoryIsEmptyException ee => string.Format(LOC.Error.PZSourceDirectoryIsEmpty, ee.DirectoryPath),
            _ => string.Format(LOC.Error.Message, ex.Message),
        };
    }

    public static string CatchException(Exception ex)
    {
        try
        {
            Logger.Instance.Error(ex);
            return FormatException(ex);
        }
        catch
        {
            // DO NOTHING
            return "";
        }
    }
}
