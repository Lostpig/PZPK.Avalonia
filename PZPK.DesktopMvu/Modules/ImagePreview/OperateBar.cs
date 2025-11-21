using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using PZPK.Desktop.Common;
using System;
using Avalonia.Layout;
using System.Linq;

namespace PZPK.Desktop.Modules.ImagePreview;
using static PZPK.Desktop.Common.ControlHelpers;

public class OperateBar(PreviewModel vm) : ComponentBase<PreviewModel>(vm)
{
    protected override StyleGroup? BuildStyles()
    {
        return [
            new Style<StackPanel>(s => s.Class("container"))
                .Opacity(0),
            new Style<StackPanel>(s => s.Class("container").Class(":pointerover"))
                .Opacity(1),
            new Style<StackPanel>(s => s.Class("items-stack").Child())
                .Margin(5, 0)
                .VerticalAlignment(VerticalAlignment.Center),
            new Style<Button>()
                .Padding(10, 4)
        ];
    }

    protected override object Build(PreviewModel vm)
    {
        var bgColor = App.Instance.Suki.GetSukiColor("SukiControlBorderBrush");

        return VStackPanel(HorizontalAlignment.Center).Classes("container")
                .Background(new SolidColorBrush(Colors.Transparent))
                .Children(
                    new Border()
                        .Padding(5)
                        .CornerRadius(0, 10)
                        .Background(bgColor)
                        .Child(
                            HStackPanel()
                                .Height(40)
                                .Classes("items-stack")
                                .Children(
                                    PzButton("-").OnClick(_ => ScaleChange?.Invoke(-0.1)),
                                    TextedBlock(() => vm.ScalePercent),
                                    PzButton("+").OnClick(_ => ScaleChange?.Invoke(0.1)),
                                    PzButton("OriginalSize").OnClick(_ => ToOriginSize?.Invoke()),
                                    PzButton("FitToWidth").OnClick(_ => FitToWidth?.Invoke()),
                                    PzButton("FitToHeight").OnClick(_ => FitToHeight?.Invoke()),
                                    PZSeparatorH(),
                                    PzButton("Prev").OnClick(_ => ImageChange?.Invoke(-1)),
                                    PzButton("Next").OnClick(_ => ImageChange?.Invoke(1)),
                                    PZSeparatorH(),
                                    new ComboBox().Width(120)
                                        .SelectedIndex(() => (int)vm.Lock, i => vm.Lock = (LockMode)i)
                                        .Items(
                                            new ComboBoxItem().Content("None"),
                                            new ComboBoxItem().Content("Lock scale"),
                                            new ComboBoxItem().Content("Lock fit to width"),
                                            new ComboBoxItem().Content("Lock fit to height")
                                        ),
                                    PZSeparatorH(),
                                    PzButton("FullScreen").OnClick(_ => ToggleFullScreen?.Invoke())
                                )
                        )
                );
    }

    public event Action<double>? ScaleChange;
    public event Action<int>? ImageChange;
    public event Action? ToggleFullScreen;
    public event Action? ToOriginSize;
    public event Action? FitToWidth;
    public event Action? FitToHeight;
}
