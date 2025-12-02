using SukiUI.Dialogs;
using System.Threading.Tasks;

namespace PZPK.Desktop.Main;

internal static class Dialog
{
    public static async Task<bool> Confirm(string message, string title = "Confirm", string yesButton = "Yes", string noButton = "No")
    {
        var result = await App.Instance.MainWindow.DialogManager
            .CreateDialog()
            .WithTitle(title)
            .WithContent (message)
            .WithYesNoResult(yesButton, noButton)
            .TryShowAsync();

        return result;
    }
}
