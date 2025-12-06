using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using PZPK.Desktop.Common;
using System;

namespace PZPK.Desktop.ImagePreview;
using static PZPK.Desktop.Common.ControlHelpers;

public class InfoBar(PreviewModel vm) : ComponentBase<PreviewModel>(vm)
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
                .VerticalAlignment(VerticalAlignment.Center)
        ];
    }
    protected override object Build(PreviewModel vm)
    {
        if (vm is null) throw new InvalidOperationException("ViewModel cannot be null");
        var bgColor = App.Instance.Suki.GetSukiColor("SukiDialogBackground");

        return VStackPanel(HorizontalAlignment.Center).Classes("container")
            .Background(new SolidColorBrush(Colors.Transparent))
            .Children(
                new Border()
                    .Padding(25, 4, 25, 8)
                    .CornerRadius(10, 0)
                    .Background(bgColor)
                    .Child(
                        HStackPanel()
                            .Height(40)
                            .Classes("items-stack")
                            .Children(
                                PzText(() => vm.FileName),
                                PzSeparatorH(),
                                PzText(() => vm.Current.ToString()),
                                PzText("/"),
                                PzText(() => vm.Total.ToString()),
                                PzSeparatorH(),
                                PzText(() => vm.SizeText),
                                PzSeparatorH(),
                                PzText(() => vm.FileSizeText)
                            )
                    )
            );
    }
}
