namespace PZPK.Desktop.Main;

public abstract class PageModelBase
{
    public PZToast Toast { get; } = App.Instance.MainWindow.Toast;
    public PZDialog Dialog { get; } = App.Instance.MainWindow.Dialog;

    public void OnErrorCatch(Exception ex)
    {
        var msg = ErrorProxy.CatchException(ex);
        Toast.Error(msg);
    }
}
