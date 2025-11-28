using SukiUI.Toasts;
using System;

namespace PZPK.Desktop.Main;

internal static class Toast
{
    public static void ShowToast(string title, string message, int time = 3)
    {
        App.Instance.MainWindow.ToastManager.CreateToast()
            .WithTitle(title)
            .WithContent(message)
            .Dismiss().After(TimeSpan.FromSeconds(time))
            .Queue();
    }

    public static void Error(string message) => ShowToast("Error", message);
}
