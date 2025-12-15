using Avalonia.Interactivity;
using System.Reactive.Subjects;

namespace PZPK.Desktop.Common;

internal class PZRadioGroup<T> : ContentControl
{
    public string RadioGroupName { get; private set; }
    private Panel Container => (Content as Panel)!;
    private IEnumerable<T>? _source;
    private Func<T, RadioButton>? _build;
    private ISubject<T>? _subject;
    private IDisposable? _subscription;
    private T? _cacheValue;
    public List<RadioButton> Items = [];

    public PZRadioGroup(string name)
    {
        RadioGroupName = name;
        Content = new Panel();
    }

    public PZRadioGroup<T> SetItemsPanel(Panel panel)
    {
        Content = panel;
        return this;
    }
    public PZRadioGroup<T> SetItemTemplete(Func<T, RadioButton> build)
    {
        _build = build;
        Render();
        return this;
    }
    public PZRadioGroup<T> SetItemsSource(IEnumerable<T> sources) 
    {
        _source = sources;
        Render();
        return this;
    }
    public PZRadioGroup<T> CheckedItem(ISubject<T> subject)
    {
        _subject = subject;
        _subscription?.Dispose();

        _subscription = _subject.Subscribe(t =>
        {
            if (!Equals(t, _cacheValue)) SetCheckValue(t);
        });

        return this;
    }

    private void SetCheckValue(T value)
    {
        foreach(var item in Container.Children)
        {
            var radio = (RadioButton)item;
            if (Equals(radio.DataContext, value)) 
            {
                radio.IsChecked = true;
                _cacheValue = value;
                _subject?.OnNext(value);
                break;
            }
        }
    }
    public void Render()
    {
        if (_source != null && _build != null)
        {
            Container.Children.Clear();
            foreach(var s in _source)
            {
                var item = _build(s);
                item.IsCheckedChanged += Item_IsCheckedChanged;

                item.DataContext = s;
                Container.Children.Add(item);
            }
        }
    }

    private void Item_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (e.Source is RadioButton radio)
        {
            if (radio.IsChecked == true)
            {
                _subject?.OnNext((T)radio.DataContext!);
            }
        }
    }
}
