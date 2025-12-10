using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;
using SukiUI.Controls;
using System.Collections.Generic;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class CreatorPage : PZComponentBase
{
    protected override object Build()
    {
        return Grid(null, "50, 1*")
            .Children(
                new Stepper()
                    .Row(0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Margin(50, 10, 50, 0)
                    .Index(() => Model.Step - 1)
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
                            new IndexPanel(Model).IsVisible(() => Model.Step == 1),
                            new PropertiesPanel(Model).IsVisible(() => Model.Step == 2),
                            new PackingPanel(Model).IsVisible(() => Model.Step == 3),
                            new CompletePanel(Model).IsVisible(() => Model.Step == 4)
                        )
                    )
            );
    }
    protected override void OnCreated()
    {
        base.OnCreated();
        Model.OnStepChanged += StateHasChanged;
    }

    private readonly CreatorModel Model = CreatorModel.Instance;
}
