using Avalonia.Markup.Declarative;
using System;
using System.Collections.Generic;
using System.Text;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class PropertiesPanel : ComponentBase
{
    override protected object Build()
    {
        return PzText("Properties Panel");
    }
}
