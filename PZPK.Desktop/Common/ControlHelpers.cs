using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;
using SukiUI.Controls;
using System.Collections;
using System.Reactive.Subjects;

namespace PZPK.Desktop.Common;

internal static class ControlHelpers
{
    public static StackPanel VStackPanel(HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        return new StackPanel
        {
            Orientation = Orientation.Vertical,
            HorizontalAlignment = alignment,
        };
    }

    public static StackPanel HStackPanel(VerticalAlignment alignment = VerticalAlignment.Center)
    {
        return new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = alignment
        };
    }

    public static TextBlock PzText(Func<string> func, params string[] classes)
    {
        var ctrl = new TextBlock().Text(func);
        foreach (var c in classes) ctrl.Classes.Add(c);
        return ctrl;
    }
    public static TextBlock PzText(string text, params string[] classes)
    {
        var ctrl = new TextBlock() { Text = text };
        foreach (var c in classes) ctrl.Classes.Add(c);
        return ctrl;
    }
    public static TextBlock PzText(IObservable<string> obs, params string[] classes)
    {
        var ctrl = new TextBlock().Text(obs);
        foreach (var c in classes) ctrl.Classes.Add(c);
        return ctrl;
    }

    public static TextBox PzTextBox(ISubject<string> subject)
    {
        return new TextBox().Text(subject);
    }
    public static TextBox PzReadOnlyTextBox(Func<string> getter)
    {
        return new TextBox().Text(getter).IsReadOnly(true);
    }
    public static TextBox PzReadOnlyTextBox(IObservable<string> obs)
    {
        return new TextBox().Text(obs).IsReadOnly(true);
    }

    public static Button SukiButton(string text, params string[] classes)
    {
        var btn = new Button()
        {
            Content = text,
        };

        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button SukiButton(Func<string> textGetter, params string[] classes)
    {
        var btn = new Button().Content(textGetter);
        foreach (var c in classes) btn.Classes.Add(c);

        return btn;
    }
    public static Button IconButton(MaterialIconKind icon, int size = 40)
    {
        var btn = new Button()
        {
            Padding = new Avalonia.Thickness(0),
            Width = size,
            Height = size,
            Content = new MaterialIcon()
            {
                Kind = icon,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };
        btn.Classes.Add("Rounded");

        return btn;
    }

    public static Grid Grid(string? colDefines = null, string? rowDefines = null)
    {
        var grid = new Grid();
        if (!string.IsNullOrEmpty(colDefines))
            grid.ColumnDefinitions = new ColumnDefinitions(colDefines);
        if (!string.IsNullOrEmpty(rowDefines))
            grid.RowDefinitions = new RowDefinitions(rowDefines);

        return grid;
    }

    public static MaterialIcon MaterialIcon(MaterialIconKind kind, int size = 24)
    {
        return new MaterialIcon
        {
            Kind = kind,
            Width = size,
            Height = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
    }
    public static MaterialIcon MaterialIcon(Func<MaterialIconKind> func, int size = 24)
    {
        var icon = new MaterialIcon
        {
            Width = size,
            Height = size,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        static void setter(MaterialIcon i, MaterialIconKind v) => i.Kind = v;

        icon._set(setter, func);

        return icon;
    }

    public static Rectangle PzSeparatorV(int height = 1, IBrush? color = null)
    {
        color ??= App.Instance.Suki.GetSukiColor("SukiControlBorderBrush");
        return new Rectangle()
        {
            Height = height,
            Fill = color,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }
    public static Rectangle PzSeparatorH(int width = 1, IBrush? color = null)
    {
        color ??= App.Instance.Suki.GetSukiColor("SukiControlBorderBrush");
        return new Rectangle()
        {
            Width = width,
            Fill = color,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }

    public static Stepper Index(this Stepper control, IObservable<int> obs)
    {
        static void setter(Stepper c, int v) => c.Index = v;
        control._set(setter, obs);

        return control;
    }
    public static Stepper Steps(this Stepper control, Func<IEnumerable> func)
    {
        static void setter(Stepper c, IEnumerable v) => c.Steps = v;
        control._set(setter, func);

        return control;
    }
    public static Stepper AlternativeStyle(this Stepper control, bool value)
    {
        control.AlternativeStyle = value;

        return control;
    }
}
