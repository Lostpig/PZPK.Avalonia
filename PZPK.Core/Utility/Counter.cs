namespace PZPK.Core.Utility;

public class Counter
{
    private readonly int _startNumber;
    private int _count;
    public Counter(int startNumber)
    {
        _startNumber = startNumber;
        _count = 0;
    }
    public int Next()
    {
        _count++;
        return _count + _startNumber;
    }
    public void Reset()
    {
        _count = 0;
    }
}
