using Avalonia.Markup.Declarative;
using SukiUI.Controls;

namespace PZPK.Desktop;
using static Common.ControlHelpers;

internal class MainContent(Routes routes) : ComponentBase<Routes>(routes)
{
    protected override object Build(Routes? routes)
    {
        var m = new SukiSideMenu()
        {
            IsSearchEnabled = false
        };

        foreach (var p in Routes.Pages)
        {
            var item = new SukiSideMenuItem()
            {
                Icon = MaterialIcon(p.Icon),
                Header = p.PageName,
                PageContent = p
            };

            m.Items.Add(item);
        }

        return m;
    }

}
