namespace PZPK.Core.Utility;

internal class StreamBlockWrapper
{
    private readonly Stream _innerStream;
    private readonly long _offset;
    private readonly long _length;
    private readonly int _blockSize;
    private readonly int _count;

    public int Count => _count;
    public int BlockSize => _blockSize;
    public long Length => _length;

    public StreamBlockWrapper(Stream innerStream, long offset, long length, int blockSize)
    {
        _innerStream = innerStream;
        _offset = offset;
        _length = length;
        _blockSize = blockSize;
        _count = ComputeBlockCount();
    }
    private int ComputeBlockCount()
    {
        int count = (int)(_length / _blockSize);
        if (_length % _blockSize != 0) count += 1;
        return count;
    }
    private int GetBlockSize(int blockIndex)
    {
        if (_length % _blockSize == 0 || blockIndex < _count - 1)
        {
            return _blockSize;
        }

        return (int)(_length % _blockSize);
    }
    private long GetBlockOffset(int blockIndex)
    {
        return _blockSize * blockIndex + _offset;
    }
    public int ReadBlock(int blockIndex, byte[] output)
    {
        return ReadBlock(blockIndex, output.AsSpan());
    }
    public int ReadBlock(int blockIndex, Span<byte> output)
    {
        if (blockIndex < 0 || blockIndex >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(blockIndex));
        }

        var currentBlockOffset = GetBlockOffset(blockIndex);
        var currentBlockSize = GetBlockSize(blockIndex);
        var writen = output.Slice(0, currentBlockSize);

        _innerStream.Seek(currentBlockOffset, SeekOrigin.Begin);
        return _innerStream.Read(writen);
    }
}

