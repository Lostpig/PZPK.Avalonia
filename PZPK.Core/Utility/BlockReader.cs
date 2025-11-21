namespace PZPK.Core.Utility;

internal class BlockReader
{
    private readonly StreamBlockWrapper _wrapper;
    private int _position;
    private long _bytesRead;
    private readonly Action<long, long>? _progress;

    public int Count => _wrapper.Count;
    public int BlockSize => _wrapper.BlockSize;
    public int Position => _position;
    public bool End => Position >= Count;

    public BlockReader(StreamBlockWrapper wrapper, Action<long, long>? progressHandler = default)
    {
        _wrapper = wrapper;
        _position = 0;
        _bytesRead = 0;
        _progress = progressHandler;
    }
    public int ReadNext(byte[] buffer)
    {
        return ReadNext(buffer.AsSpan());
    }
    public int ReadNext(Span<byte> buffer)
    {
        if (End) return 0;
        var bytesRead = _wrapper.ReadBlock(_position, buffer);
        _position++;
        _bytesRead += bytesRead;
        _progress?.Invoke(_bytesRead, _wrapper.Length);

        return bytesRead;
    }
}

