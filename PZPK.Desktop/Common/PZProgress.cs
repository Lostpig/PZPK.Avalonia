namespace PZPK.Desktop.Common;

public class PZProgress<T> : IProgress<T>
{
    /// <summary>
    /// Represents the elapsed time, in seconds, since the last throttle event.
    /// </summary>
    public double ThrottleElapsed { get; set; } = 0.5;

    private DateTime lastReportTime = DateTime.MinValue;
    public event EventHandler<T>? ProgressChanged;
    public void Report(T value)
    {
        if ((DateTime.Now - lastReportTime).TotalSeconds < ThrottleElapsed)
            return;

        lastReportTime = DateTime.Now;
        ProgressChanged?.Invoke(this, value);
    }
}
