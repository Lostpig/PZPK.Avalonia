using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;
using SukiUI.Controls;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using System.Collections.Generic;
using PZPK.Desktop.Main.Creator;
using System.Linq;

namespace PZPK.Desktop.Modules.Creator;
using static Common.ControlHelpers;

public class CreatorPage : ComponentBase
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
                            new PropertiesPanel().IsVisible(() => Model.Step == 2),
                            new PackingPanel().IsVisible(() => Model.Step == 3)
                        )
                    )
            );
    }

    private CreatorModel Model = new();
}
