using Avalonia.Controls.Notifications;
using SukiUI.Dialogs;
using System.Threading.Tasks;

namespace PZPK.Desktop.Global;

public record DialogOptions<T>
{
    public string Title { get; set; } = "";
    public object Content { get; set; } = "";
    public NotificationType Type { get; set; } = NotificationType.Information;
    public (string text, T result, bool dismiss)[] Buttons { get; set; } = [];
    public string[][]? ButtonStyles { get; set; }
}

public class PZDialog(ISukiDialogManager manager)
{
    public ISukiDialogManager Manager { get; init; } = manager;

    public Task<T> ShowDialog<T>(DialogOptions<T> options)
    {
        var builder = Manager.CreateDialog()
            .WithTitle(options.Title)
            .WithContent(options.Content);

        var completion = new TaskCompletionSource<T>();
        for (int i = 0; i < options.Buttons.Length; i++)
        {
            var btn = options.Buttons[i];
            var style = options.ButtonStyles?.Length > i ? options.ButtonStyles[i] : [];
            builder.AddActionButton(btn.text, d =>
            {
                completion.SetResult(btn.result);
            }, btn.dismiss, style);
        }
        builder.TryShow();

        return completion.Task;
    }

    public static DialogOptions<bool> ConfirmOptions(string title, object content)
    {
        return new DialogOptions<bool>
        {
            Title = title,
            Content = content,
            Type = NotificationType.Information,
            Buttons = [
                (LOC.Base.OK, true, true),
                (LOC.Base.Cancel, false, true),
            ],
            ButtonStyles = [
                [],
                ["Accent"]
            ]
        };
    }
    public static DialogOptions<bool> AlertOptions(string title, object content)
    {
        return new DialogOptions<bool>
        {
            Title = title,
            Content = content,
            Type = NotificationType.Warning,
            Buttons = [
                (LOC.Base.OK, true, true)
            ],
            ButtonStyles = [
                []
            ]
        };
    }
}
