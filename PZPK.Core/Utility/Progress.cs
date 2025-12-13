namespace PZPK.Core.Utility;

public record PZProgressState
{
    /**
     * 总文件个数
     */
    public int Files { get; set; }
    /**
     * 已处理文件个数
     */
    public int ProcessedFiles { get; set; }
    /**
     * 总字节数
     */
    public long Bytes { get; set; }
    /**
     * 已处理字节数
     */
    public long ProcessedBytes { get; set; }
    /**
     * 当前文件的字节数
     */
    public long CurrentBytes { get; set; }
    /**
     * 当前文件已处理的字节数
     */
    public long CurrentProcessedBytes { get; set; }

    public PZProgressState()
    {
        Files = 0;
        ProcessedFiles = 0;
        Bytes = 0;
        ProcessedBytes = 0;
        CurrentBytes = 0;
        CurrentProcessedBytes = 0;
    }
    public PZProgressState(int files, long bytes)
    {
        Files = files;
        Bytes = bytes;
    }

    public void Reset()
    {
        Files = 0;
        ProcessedFiles = 0;
        Bytes = 0;
        ProcessedBytes = 0;
        CurrentBytes = 0;
        CurrentProcessedBytes = 0;
    }
    public void CopyFrom(PZProgressState other)
    {
        Files = other.Files;
        ProcessedFiles = other.ProcessedFiles;
        Bytes = other.Bytes;
        ProcessedBytes = other.ProcessedBytes;
        CurrentBytes = other.CurrentBytes;
        CurrentProcessedBytes = other.CurrentProcessedBytes;
    }
}
