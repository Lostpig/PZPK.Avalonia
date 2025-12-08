using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using PZPK.Desktop.Common;
using System;

namespace PZPK.Desktop.ImagePreview;
using static PZPK.Desktop.Common.ControlHelpers;

public class InfoBar: ComponentBase
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
    protected override object Build()
    {
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
                                PzText(() => Model.FileName),
                                PzSeparatorH(),
                                PzText(() => Model.Current.ToString()),
                                PzText("/"),
                                PzText(() => Model.Total.ToString()),
                                PzSeparatorH(),
                                PzText(() => Model.SizeText),
                                PzSeparatorH(),
                                PzText(() => Model.FileSizeText)
                            )
                    )
            );
    }

    private readonly PreviewModel Model;
    public InfoBar(PreviewModel model) : base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        Initialize();
    }
}
