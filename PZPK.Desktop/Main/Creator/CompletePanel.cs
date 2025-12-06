using Avalonia.Markup.Declarative;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class CompletePanel(CreatorModel vm) : ComponentBase<CreatorModel>(vm)
{
    protected override object Build(CreatorModel? vm)
    {
        return PzText("Packing complete!");
    }
}
