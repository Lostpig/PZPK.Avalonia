using Avalonia.Layout;
using SukiUI.Controls;
using System.Reactive.Linq;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class CreatorPage : PZComponentBase
{
    protected override Control Build()
    {
        return Grid(null, "50, 1*")
            .Children(
                new Stepper()
                    .Row(0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Margin(50, 10, 50, 0)
                    .Index(Model.Step.Select(s => s - 1))
                    .Steps(() => new List<string>()
                    {
                        "Index",
                        "Properties",
                        "Packing",
                        "Complete"
                    }),
                new GlassCard()
                    .Row(1)
                    .Margin(20, 5, 20, 20)
                    .Content(
                        Grid().Children(
                            new IndexPanel().IsVisible(Model.Step.Select(s => s == 1)),
                            new PropertiesPanel(Model).IsVisible(Model.Step.Select(s => s == 2)),
                            new PackingPanel(Model).IsVisible(Model.Step.Select(s => s == 3)),
                            new CompletePanel().IsVisible(Model.Step.Select(s => s == 4))
                        )
                    )
            );
    }

    private readonly CreatorModel Model = CreatorModel.Instance;
}
