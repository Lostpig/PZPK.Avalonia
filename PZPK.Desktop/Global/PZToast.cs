using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;

namespace PZPK.Desktop.Global;

public class PZToast
{
    public ISukiToastManager Manager { get; init; }
    public PZToast(ISukiToastManager manager)
    {
        Manager = manager;
    }

    public void ShowToast(string title, string message, NotificationType toastType, int time = 3)
    {
        Manager.CreateToast()
            .WithTitle(title)
            .WithContent(message)
            .OfType(toastType)
            .Dismiss().After(TimeSpan.FromSeconds(time))
            .Queue();
    }

    public void Error(string message, string title = "Error") => ShowToast(title, message, NotificationType.Error);
    public void Warning(string message, string title = "Warning") => ShowToast(title, message, NotificationType.Warning);
    public void Info(string message, string title = "Information") => ShowToast(title, message, NotificationType.Information);
    public void Success(string message, string title = "Success") => ShowToast(title, message, NotificationType.Success);
}
