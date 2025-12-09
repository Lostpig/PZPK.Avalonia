using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using SukiUI.Controls;
using SukiUI.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PZPK.Desktop.Global;

public class PZDialog
{
    public ISukiDialogManager Manager { get; init; }
    public PZDialog(ISukiDialogManager manager)
    {
        Manager = manager;
    }

    public async Task<bool> Confirm(
        NotificationType msgType, 
        string title, 
        string message, 
        string yesButton, 
        string noButton, 
        string[] yesClasses, 
        string[] noClasses)
    {
        var builder = Manager.CreateDialog()
             .OfType(msgType)
             .WithTitle(title)
             .WithContent(message);

        builder.Completion = new TaskCompletionSource<bool>();
        builder.AddActionButton(yesButton, delegate
        {
            builder.Completion.SetResult(result: true);
        }, dismissOnClick: true, yesClasses);
        builder.AddActionButton(noButton, delegate
        {
            builder.Completion.SetResult(result: false);
        }, dismissOnClick: true, noClasses);

        return await builder.TryShowAsync();
    }

    public async Task<bool> DeleteConfirm(string message, string title = "Delete")
    {
        return await Confirm(
            NotificationType.Warning,
            title,
            message,
            "Delete",
            "Cancel",
            ["Danger"],
            []
        );
    }
    public async Task<bool> InfoConfirm(string message, string title = "Infomation")
    {
        return await Confirm(
            NotificationType.Information,
            title,
            message,
            "Yes",
            "No",
            [],
            []
        );
    }
    public async Task<bool> WarningConfirm(string message, string title = "Warning")
    {
        return await Confirm(
            NotificationType.Warning,
            title,
            message,
            "Yes",
            "No",
            ["Warning"],
            []
        );
    }

    public bool Alert(string message, string title = "Message", NotificationType msgType = NotificationType.Information)
    {
        return Manager.CreateDialog()
            .OfType(msgType)
            .WithTitle(title)
            .WithContent(message)
            .WithActionButton("Close", _ => { }, true, "Flat")
            .TryShow();
    }

    public void ShowContentDialog(string title, object content)
    {
        Manager.CreateDialog()
            .WithTitle(title)
            .WithContent(content)
            .WithActionButton("Close", _ => { }, true)
            .TryShow();
    }
}
