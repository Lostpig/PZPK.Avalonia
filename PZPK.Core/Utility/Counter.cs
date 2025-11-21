namespace PZPK.Core.Utility;

public class Counter
{
    private int _count;
    public Counter(int startNumber)
    {
        _count = startNumber;
    }
    public int Next()
    {
        _count++;
        return _count;
    }
}
