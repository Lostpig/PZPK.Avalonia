using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace PZPK.Desktop.ImagePreview;
using static PZPK.Desktop.Common.ControlHelpers;

public class OperateBar: PZComponentBase
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

    protected override Control Build()
    {
        var bgColor = App.Instance.Suki.GetSukiColor("SukiControlBorderBrush");

        return VStackPanel(HorizontalAlignment.Center).Classes("container")
                .Background(Brushes.Transparent)
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
                                    SukiButton("-").OnClick(_ => Model.Scale.Reducer(s => s - 0.1)),
                                    PzText(Model.ScalePercent),
                                    SukiButton("+").OnClick(_ => Model.Scale.Reducer(s => s + 0.1)),
                                    SukiButton("OriginalSize").OnClick(_ => Model.Scale.OnNext(1)),
                                    SukiButton("FitToWidth").OnClick(_ => Model.FitToWidth()),
                                    SukiButton("FitToHeight").OnClick(_ => Model.FitToHeight()),
                                    PzSeparatorH(),
                                    SukiButton("Prev").OnClick(_ => Model.Current.Reducer(i => i - 1)),
                                    SukiButton("Next").OnClick(_ => Model.Current.Reducer(i => i + 1)),
                                    PzSeparatorH(),
                                    new ComboBox()
                                        .Width(120)
                                        .ItemsSource(Enum.GetValues<LockMode>())
                                        .SelectedItem(subject: Model.Lock)
                                        .ItemTemplate<LockMode>(l => PzText(PreviewModel.LockModeName(l))),
                                    PzSeparatorH(),
                                    SukiButton("FullScreen").OnClick(_ => Model.FullScreen.Reducer(f => !f))
                                )
                        )
                );
    }

    private static PreviewModel Model => PreviewModel.Instance;
}
