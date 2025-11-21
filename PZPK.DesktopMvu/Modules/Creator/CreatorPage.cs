using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.Markup.Declarative;
using PZPK.Desktop.Common;
using SukiUI.Controls;
using Avalonia.Media;
using Avalonia.Controls.Primitives;
using System.Collections.Generic;

namespace PZPK.Desktop.Modules.Creator;
using static Common.ControlHelpers;

public class CreatorPage : ComponentBase
{
    protected override StyleGroup? BuildStyles()
    {
        return [
            new Style<StackPanel>(s => s.Class("properties-stack").Child())
                .Margin(0, 0, 0, 8)
                .Height(50),
            new StyleGroup(s => s.Class("properties-stack")) {
                new Style<TextBlock>(s => s.Class("label"))
                    .VerticalAlignment(VerticalAlignment.Center)
                    .TextAlignment(TextAlignment.Right)
                    .Padding(10, 0)
                    .Width(150),
                new Style<TextBox>()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Width(350),
            }
        ];
    }

    private GlassCard BuildPropertiesPanel()
    {
        return new GlassCard()
            .Content(
                Grid(null, "1*, 80")
                .Children(
                    new ScrollViewer().Row(0)
                    .VerticalScrollBarVisibility(ScrollBarVisibility.Auto)
                    .Content(
                        VStackPanel()
                            .Classes("properties-stack")
                            .Children(
                                HStackPanel().Children(
                                    TextedBlock("Name").Classes("label"),
                                    PzTextBox(() => State.Name, v => State.Name = v)
                                ),
                                HStackPanel().Children(
                                    TextedBlock("Description").Classes("label"),
                                    PzTextBox(() => State.Description, v => State.Description = v)
                                ),
                                HStackPanel().Children(
                                    TextedBlock("Password").Classes("label"),
                                    PzTextBox(() => State.Password, v => State.Password = v)
                                ),
                                HStackPanel().Children(
                                    TextedBlock("Confirm Password").Classes("label"),
                                    PzTextBox(() => State.RptPassword, v => State.RptPassword = v)
                                ),
                                HStackPanel().Children(
                                    TextedBlock("Tags").Classes("label"),
                                    TextedBlock(State.TagsText)
                                ),
                                HStackPanel().Children(
                                    TextedBlock("").Classes("label"),
                                    PzTextBox(() => State.InputTag, v => State.InputTag = v).Width(100),
                                    SukiButton("Add Tag").VerticalAlignment(VerticalAlignment.Center)
                                )
                            )
                    ),
                    VStackPanel(HorizontalAlignment.Right)
                        .Row(1)
                        .Children(
                            SukiButton("Next", "Flat").VerticalAlignment(VerticalAlignment.Center)
                        )
                )
            );
    }

    protected override object Build()
    {
        return HStackPanel()
            .Margin(20)
            .Children(
                BuildPropertiesPanel()
            );
    }

    private CreatorPageState State { get; init; } = new();
}

internal enum CreateStep
{
    Properties,
    Explorer,
    TextEdit,
    Packing
}
internal class CreatorPageState
{
    public CreateStep Step { get; set; } = CreateStep.Properties;

    public string Name = "";
    public string Description = "";
    public string InputTag = "";
    public List<string> Tags = [];
    public string TagsText = "";
    public int BlockSize = 64;
    public string Password = "";
    public string RptPassword = "";

    public CreatorPageState()
    {
        // TagsText = new(() => string.Join(',', Tags.Value));
    }
}
