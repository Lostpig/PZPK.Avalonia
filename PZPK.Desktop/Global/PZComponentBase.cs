namespace PZPK.Desktop.Global;

public abstract class PZComponentBase: ComponentBase
{
    protected PZComponentBase(): base()
    {
        App.Instance.Translate.LanguageChanged += OnLanguageChanged;
    }

    protected void OnLanguageChanged()
    {
        UpdateState();
    }
}
