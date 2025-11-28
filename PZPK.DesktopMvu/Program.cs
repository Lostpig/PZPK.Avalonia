using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Declarative;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Material.Icons.Avalonia;
using SukiUI;
using System;

namespace PZPK.Desktop;

internal sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp(args);

    public static void BuildAvaloniaApp(string[] args)
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime { Args = args, ShutdownMode = ShutdownMode.OnLastWindowClose };

        var app = AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .WithInterFont()
            // .LogToTrace()
            .AfterSetup(b =>
            {
                var EditorStyle = new StyleInclude((Uri?)null) { Source = new Uri("avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml") };

                b.Instance?.Resources.Add("ControlContentThemeFontSize", 14.0);
                b.Instance?.Resources.Add("ContentControlThemeFontFamily", FontFamily.Parse("Consolas"));

                b.Instance?.Styles.Add(new SukiTheme() { ThemeColor = SukiUI.Enums.SukiColor.Blue });
                b.Instance?.Styles.Add(new MaterialIconStyles(null));
                b.Instance?.Styles.Add(EditorStyle);
                b.Instance?.DataTemplates.Add(new Main.PageLocator());
            })
#if DEBUG
            .UseHotReload()
#endif
            .SetupWithLifetime(lifetime);

        // if (OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsLinux())
        // {
        //     //app.UseManagedSystemDialogs();
        // }

        lifetime.MainWindow = App.Instance.MainWindow;
#if DEBUG
        lifetime.MainWindow?.AttachDevTools();
#endif
        lifetime.Start(args);
    }
}
