using SukiUI.Controls;
using SukiUI.Dialogs;
using SukiUI.Toasts;

namespace PZPK.Desktop.Global;

public abstract class PZWindowBase : SukiWindow
{
    public ISukiToastManager ToastManager { get; init; }
    public ISukiDialogManager DialogManager { get; init; }

    public PZDialog Dialog { get; init; }
    public PZToast Toast { get; init; }

    public PZWindowBase() : base()
    {
        ToastManager = new SukiToastManager();
        Toast = new PZToast(ToastManager);
        DialogManager = new SukiDialogManager();
        Dialog = new PZDialog(DialogManager);
        
        var ToastHost = new SukiToastHost
        {
            Manager = ToastManager
        };
        Hosts.Add(ToastHost);
        var DialogHost = new SukiDialogHost()
        {
            Manager = DialogManager
        };
        Hosts.Add(DialogHost);
    }
}
