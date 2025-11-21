using PZPK.Core.Crypto;
using PZPK.Core.Exceptions;
using PZPK.Core.Utility;
using System.Diagnostics;
using System.Text;

namespace PZPK.Core.Note;


public record PZNoteHeader(int Version,
        PZType Type,
        byte[] Sign,
        byte[] PasswordCheck,
        DateTime SavedTime,
        int FileSize,
        int IndexSize,
        int IndexOffset)
{
    public const int Size = 124;
}

public class NoteBook : IDisposable
{
    private Counter IdCounter;
    private IPZCrypto Crypto;
    public string FilePath { get; init; }
    public List<Note> Notes { get; init; }

    private NoteBook(string filePath, IPZCrypto crypto)
    {
        IdCounter = new(1);
        FilePath = filePath;
        Crypto = crypto;
        Notes = [];
    }
    private NoteBook(string filePath, IPZCrypto crypto, List<Note> notes)
    {
        FilePath = filePath;
        Crypto = crypto;

        int maxId = 0;
        Notes = [];
        foreach (var note in notes)
        {
            Notes.Add(note);
            maxId = Math.Max(maxId, note.Id);
        }

        IdCounter = new(maxId + 1);
    }

    public void Save()
    {
        using FileStream stream = File.Open(FilePath, FileMode.Create);
        stream.Seek(PZNoteHeader.Size, SeekOrigin.Begin);

        Span<byte> noteDataBuffer = new byte[Constants.Sizes.t_256KB];
        using MemoryStream indexStream = new();
        foreach (var note in Notes)
        {
            // write note data
            var length = note.Encode(noteDataBuffer);
            var encryptedNoteData = noteDataBuffer[length..];
            var encLength = Crypto.Encrypt(noteDataBuffer[..length], encryptedNoteData);
            int noteOffset = (int)stream.Position;
            stream.Write(encryptedNoteData[..encLength]);
            // write index data
            var indexPart = noteDataBuffer[(length + encLength)..];
            var titleLength = Encoding.UTF8.GetBytes(note.Title, indexPart[16..]);
            int partLength = 4 + 4 + 4 + 4 + titleLength;
            BitConverter.TryWriteBytes(indexPart[..4], partLength);
            BitConverter.TryWriteBytes(indexPart[4..8], note.Id);
            BitConverter.TryWriteBytes(indexPart[8..12], noteOffset);
            BitConverter.TryWriteBytes(indexPart[12..16], encLength);
            indexStream.Write(indexPart[..partLength]);
        }

        int indexOffset = (int)stream.Position;
        var indexData = noteDataBuffer[..(int)indexStream.Length];
        indexStream.Seek(0, SeekOrigin.Begin);
        indexStream.ReadExactly(indexData);
        var encrptedIndexData = noteDataBuffer[(int)indexStream.Length..];
        var encrptedIndexLength = Crypto.Encrypt(indexData, encrptedIndexData);
        stream.Write(encrptedIndexData[..encrptedIndexLength]);
        int fileSize = (int)stream.Length;

        using BinaryWriter bw = new(stream, Encoding.UTF8, true);
        bw.BaseStream.Seek(0, SeekOrigin.Begin);
        bw.Write(Constants.Version);
        bw.Write((int)PZType.Note);

        Constants.GetTypeHashSign(PZType.Note, noteDataBuffer[..32]);
        bw.Write(noteDataBuffer[..32]);
        var pwCheck = noteDataBuffer[32..96];
        var pwCheckLen = Crypto.Encrypt(noteDataBuffer[..32], pwCheck);
        Debug.Assert(pwCheckLen == 64);

        bw.Write(pwCheck);
        bw.Write(DateTime.Now.ToBinary());
        bw.Write(fileSize);
        bw.Write(indexOffset);
        bw.Write(encrptedIndexLength);

        stream.Flush();
    }
    public Note AddNote()
    {
        var note = new Note(IdCounter.Next(), "New Note", "");
        Notes.Add(note);

        return note;
    }
    public void DeleteNote(Note note)
    {
        Notes.Remove(note);
    }
    public void ModifyPassword(string newPassword)
    {
        var crypto = PZCrypto.Create(Constants.Version, PZCrypto.CreateKey(newPassword), Constants.Sizes.t_64KB);
        Crypto.Dispose();

        Crypto = crypto;
    }

    public void Dispose()
    {
        Crypto.Dispose();
        GC.SuppressFinalize(this);
    }

    public static NoteBook Create(string filePath, string password)
    {
        var crypto = PZCrypto.Create(Constants.Version, PZCrypto.CreateKey(password), Constants.Sizes.t_64KB);
        return new NoteBook(filePath, crypto);
    }
    public static NoteBook Open(string filePath, string password)
    {
        using var stream = File.Open(filePath, FileMode.Open);
        var header = ExtractHeader(stream);
        var crypto = PZCrypto.Create(Constants.Version, PZCrypto.CreateKey(password), Constants.Sizes.t_64KB);
        CheckPassword(crypto, header);

        var notes = ExtractNotes(stream, header, crypto);
        return new NoteBook(filePath, crypto, notes);
    }

    private static void WriteHeader(Stream stream, PZNoteHeader header)
    {
        using BinaryWriter bw = new(stream, Encoding.UTF8, true);
        bw.BaseStream.Seek(0, SeekOrigin.Begin);

        bw.Write(header.Version);
        bw.Write((int)header.Type);
        bw.Write(header.Sign);
        bw.Write(header.PasswordCheck);
        bw.Write(header.SavedTime.ToBinary());
        bw.Write(header.FileSize);
        bw.Write(header.IndexOffset);
        bw.Write(header.IndexSize);
    }
    private static PZNoteHeader ExtractHeader(Stream stream)
    {
        using BinaryReader br = new(stream, Encoding.UTF8, true);
        br.BaseStream.Seek(0, SeekOrigin.Begin);

        int version = br.ReadInt32();
        PZType type = (PZType)br.ReadInt32();
        byte[] sign = br.ReadBytes(32);
        byte[] passwordCheck = br.ReadBytes(64);
        long savedTime = br.ReadInt64();
        int fileSize = br.ReadInt32();
        int indexOffset = br.ReadInt32();
        int indexSize = br.ReadInt32();

        return new PZNoteHeader(
                    Version: version,
                    Type: type,
                    Sign: sign,
                    PasswordCheck: passwordCheck,
                    SavedTime: DateTime.FromBinary(savedTime),
                    FileSize: fileSize,
                    IndexSize: indexSize,
                    IndexOffset: indexOffset
                );
    }
    private static List<Note> ExtractNotes(Stream stream, PZNoteHeader header, IPZCrypto crypto)
    {
        List<Note> notes = [];

        stream.Seek(header.IndexOffset, SeekOrigin.Begin);
        Span<byte> buffer = new byte[Constants.Sizes.t_256KB];

        Span<byte> enctyptedData = buffer[0..header.IndexSize];
        stream.ReadExactly(enctyptedData);
        Span<byte> indexData = new byte[header.IndexSize];
        var length = crypto.Decrypt(enctyptedData, indexData);

        int position = 0;
        while (position < length)
        {
            int partLength = BitConverter.ToInt32(indexData.Slice(position, 4));
            var partBuffer = indexData.Slice(position + 4, partLength - 4);

            int id = BitConverter.ToInt32(partBuffer[..4]);
            int offset = BitConverter.ToInt32(partBuffer[4..8]);
            int size = BitConverter.ToInt32(partBuffer[8..12]);
            string title = Encoding.UTF8.GetString(partBuffer[12..]);
            position += partLength;

            stream.Seek(offset, SeekOrigin.Begin);
            var noteData = buffer[0..size];
            stream.ReadExactly(noteData);

            Span<byte> decryptedNoteData = buffer.Slice(size, size);
            var contLength = crypto.Decrypt(noteData, decryptedNoteData);

            var content = Encoding.UTF8.GetString(decryptedNoteData[..contLength]);
            var note = new Note(id, title, content);
            notes.Add(note);
        }

        return notes;
    }
    private static void CheckPassword(IPZCrypto crypto, PZNoteHeader header)
    {
        Span<byte> pwCheck = stackalloc byte[64];
        header.PasswordCheck.CopyTo(pwCheck);

        crypto.Encrypt(header.Sign, pwCheck[16..], pwCheck[..16]);
        if (!Utils.CompareBytes(pwCheck, header.PasswordCheck))
        {
            throw new PZPasswordIncorrectException();
        }
    }
}
