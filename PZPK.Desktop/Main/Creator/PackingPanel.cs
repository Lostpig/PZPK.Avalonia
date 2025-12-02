using Avalonia.Markup.Declarative;
using System;
using System.Collections.Generic;
using System.Text;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

internal class PackingPanel : ComponentBase
{
    protected override object Build()
    {
        return PzText("Packing Panel");
    }
}
