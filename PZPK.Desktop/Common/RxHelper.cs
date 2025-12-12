using System.Reactive.Subjects;

namespace PZPK.Desktop.Common;

public class ConditionSubject<T> : ISubject<T>
{
    private readonly Subject<T> _subject;
    private readonly Func<T, bool> _predicate;

    public ConditionSubject(Func<T, bool> predicate) : base()
    {
        _predicate = predicate;
        _subject = new Subject<T>();
    }

    public void OnCompleted() => _subject.OnCompleted();
    public void OnError(Exception error) => _subject.OnError(error);
    public void OnNext(T value)
    {
        if (_predicate(value)) { _subject.OnNext(value); }
    }
    public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);
}
public class ConditionBehaviorSubject<T> : ISubject<T>
{
    private readonly BehaviorSubject<T> _subject;
    private readonly Func<T, bool> _predicate;

    public ConditionBehaviorSubject(T initValue, Func<T, bool> predicate) : base()
    {
        _predicate = predicate;
        _subject = new BehaviorSubject<T>(initValue);
    }
    public T Value => _subject.Value;

    public void Reducer(Func<T, T> reducer)
    {
        var newValue = reducer(Value);
        OnNext(newValue);
    }

    public void OnCompleted() => _subject.OnCompleted();
    public void OnError(Exception error) => _subject.OnError(error);
    public void OnNext(T value)
    {
        if (_predicate(value)) { _subject.OnNext(value); }
    }
    public IDisposable Subscribe(IObserver<T> observer) => _subject.Subscribe(observer);
}
