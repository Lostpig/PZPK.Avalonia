using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

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

    public static TextBlock TextedBlock(Func<string> func)
    {
        return new TextBlock().Text(func);
    }
    public static TextBlock TextedBlock(string text)
    {
        return new TextBlock() { Text = text };
    }

    public static TextBox PzTextBox(Func<string> func, Action<string>? onChange = null)
    {
        return new TextBox().Text(func, onChange);
    }
    
    public static Button PzButton(string text)
    {
        return new Button()
        {
            Content = text
        };
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
        void setter(MaterialIconKind v) => icon.Kind = v;

        icon._set(setter, func, null, null);

        return icon;
    }

    public static TPanel Children<TPanel>(this TPanel container, Func<ICollection<Control>> func)
        where TPanel : Panel
    {
        void setter(ICollection<Control> v)
        {
            container.Children.Clear();
            foreach (var child in v)
                container.Children.Add(child);
        }

        container._set(setter, func, null, null);

        return container;
    }

    public static Rectangle PZSeparatorV(int height = 1, IBrush? color = null)
    {
        color ??= App.Instance.Suki.GetSukiColor("SukiControlBorderBrush");
        return new Rectangle()
        {
            Height = height,
            Fill = color,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
    }
    public static Rectangle PZSeparatorH(int width = 1, IBrush? color = null)
    {
        color ??= App.Instance.Suki.GetSukiColor("SukiControlBorderBrush");
        return new Rectangle()
        {
            Width = width,
            Fill = color,
            VerticalAlignment = VerticalAlignment.Stretch
        };
    }
}
