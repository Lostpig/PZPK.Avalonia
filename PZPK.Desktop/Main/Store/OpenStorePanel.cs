using SukiUI.Controls;

namespace PZPK.Desktop.Main.Store;

internal class OpenStorePanel : PZComponentBase
{
    protected override Control Build()
    {
        return new GlassCard();
    }
}
