using PZPK.Core.Exceptions;
using System.Text;

namespace PZPK.Core.Note;

public class Note
{
    const int MaxSize = 64 * 1024; // 64 KB

    public int Id { get; init; }
    public string Title { get; private set; }
    public string Content { get; private set; }

    internal Note(int id, string title, string content)
    {
        Id = id;
        Title = title;
        Content = content;
    }
    public void Save(string title, string content)
    {
        Title = title;
        Content = content;
    }

    public byte[] Encode()
    {
        var data = Encoding.UTF8.GetBytes(Content);
        if (data.Length > MaxSize)
        {
            throw new PZNoteSizeExceededException(MaxSize, data.Length);
        }

        return data;
    }
    public int Encode(Span<byte> buffer)
    {
        var length = Encoding.UTF8.GetBytes(Content, buffer);
        if (length > MaxSize)
        {
            throw new PZNoteSizeExceededException(MaxSize, length);
        }

        return length;
    }
}
