using PZPK.Desktop.Localization;

namespace PZPK.Desktop.Global;

public abstract class PZComponentBase: ComponentBase
{
    protected PZComponentBase(): base()
    {
        Translate.LanguageChanged += OnLanguageChanged;
    }

    protected void OnLanguageChanged()
    {
        UpdateState();
    }
}
