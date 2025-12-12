using Avalonia.Collections;
using System.Collections;

namespace PZPK.Desktop.Common;

internal class PzControls<T> : IList<T> where T : Control
{
    public struct PzEnumerator : IEnumerator<T>, IEnumerator, IDisposable
    {
        private AvaloniaList<Control>.Enumerator _innerEnumerator;

        public T Current => (T)_innerEnumerator.Current;

        object? IEnumerator.Current => Current;

        public PzEnumerator(AvaloniaList<Control> inner)
        {
            _innerEnumerator = inner.GetEnumerator();
        }

        public bool MoveNext()
        {
            return _innerEnumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            ((IEnumerator)_innerEnumerator).Reset();
        }

        public void Dispose()
        {
            _innerEnumerator.Dispose();
        }
    }

    public PzControls(Controls controls)
    {
        _inner = controls;
    }

    private readonly Controls _inner;
    public int Count => _inner.Count;

    public bool IsReadOnly => throw new NotImplementedException();

    public void Add(T item)
    {
        _inner.Add(item);
    }
    public void Remove(T item)
    {
        _inner.Remove(item);
    }
    public int IndexOf(T item)
    {
        return _inner.IndexOf(item);
    }
    public void Insert(int index, T item)
    {
        _inner.Insert(index, item);
    }
    public void RemoveAt(int index)
    {
        _inner.RemoveAt(index);
    }
    public void Clear()
    {
        _inner.Clear();
    }
    public bool Contains(T item)
    {
        return _inner.Contains(item);
    }
    public void CopyTo(T[] array, int arrayIndex)
    {
        _inner.CopyTo(array, arrayIndex);
    }
    bool ICollection<T>.Remove(T item)
    {
        return _inner.Remove(item);
    }
    public IEnumerator<T> GetEnumerator()
    {
        return new PzEnumerator(_inner);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public T this[int index]
    {
        get
        {
            return (T)_inner[index];
        }
        set
        {
            T val = (T)_inner[index];
            if (!EqualityComparer<T>.Default.Equals(val, value))
            {
                _inner[index] = value;
            }
        }
    }
}
