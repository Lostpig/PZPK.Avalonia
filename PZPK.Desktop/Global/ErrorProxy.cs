using Ex = PZPK.Core.Exceptions;

namespace PZPK.Desktop.Global;

internal class ErrorProxy
{
    private static string FormatException(Exception ex)
    {
        return ex switch
        {
            Ex.CreatorInvaildException => "",
            Ex.DuplicateNameException => "",
            Ex.EmptyStringException => "",
            Ex.FileInIndexNotEncodeException => ",",
            Ex.FileTypeMismatchException => "",
            Ex.FileVersionNotCompatiblityException => "",
            Ex.OldVersionEncryptException => "",
            Ex.OutputDirectoryIsNotEmptyException => "",
            Ex.OutputFileAlreadyExistsException => "",
            Ex.PathIsNotDirectoryException => "",
            Ex.PZFileNotFoundException => "",
            Ex.PZFolderNotFoundException => "",
            Ex.PZNoteSizeExceededException => "",
            Ex.PZPasswordIncorrectException => "",
            Ex.PZSignCheckedException => "",
            Ex.SourceDirectoryIsEmptyException => "",
            _ => string.Format(LOC.Error.Message, ex.Message),
        };
    }

    public static string CatchException(Exception ex)
    {
        try
        {
            Logger.Instance.Error(ex);
        }
        catch
        {
            // DO NOTHING
        }

        return FormatException(ex);
    }
}
