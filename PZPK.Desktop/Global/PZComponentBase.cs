using PZPK.Desktop.Common;
using PZPK.Desktop.Localization;

namespace PZPK.Desktop.Global;

public abstract class PZComponentBase: ComponentBase
{
    protected static SukiHelpers Suki => App.Instance.Suki;
    protected PZComponentBase(): base()
    {
        Translate.LanguageChanged += UpdateState;
        Settings.ThemeChanged += UpdateState;
    }
    protected PZComponentBase(ViewInitializationStrategy s) : base(s)
    {
        Translate.LanguageChanged += UpdateState;
        Settings.ThemeChanged += UpdateState;
    }
}
