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
                    .Index(Model.Step.Select(s => s.current - 1))
                    .Steps(GetSteps),
                new GlassCard()
                    .Row(1)
                    .Margin(20, 5, 20, 20)
                    .Content(
                        Grid().Children(
                            new IndexPanel().IsVisible(Model.Step.Select(s => s.current == 1)),
                            new PropertiesPanel().IsVisible(Model.Step.Select(s => s.current == 2)),
                            new PackingPanel().IsVisible(Model.Step.Select(s => s.current == 3)),
                            new CompletePanel().IsVisible(Model.Step.Select(s => s.current == 4))
                        )
                    )
            );
    }

    private readonly CreatorModel Model = CreatorModel.Instance;
    private string[] GetSteps()
    {
        return [
            LOC.PZPK.CreateIndex,
            LOC.PZPK.SetProperties,
            LOC.PZPK.Packing,
            LOC.PZPK.Completed
        ];
    }
}
