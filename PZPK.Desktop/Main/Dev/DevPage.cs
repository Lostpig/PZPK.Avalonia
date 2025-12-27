using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PZ.RxAvalonia.Reactive;
using PZPK.Core;
using PZPK.Core.Crypto;
using PZPK.Desktop.Previews;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace PZPK.Desktop.Main.Dev;
using static PZPK.Desktop.Common.ControlHelpers;

internal class DevPage : PZComponentBase
{
    protected override Control Build()
    {
        return VStackPanel()
            .Children(
                HStackPanel().Margin(10)
                    .Children(
                        PzText("File"),
                        PzTextBox(SelectedFile).Width(400).IsReadOnly(true),
                        SukiButton("Select File").RxClick(OnSelectFile),
                        SukiButton("OpenAsImage").RxClick(OnOpenImage),
                        SukiButton("OpenAsVideo").RxClick(OnOpenVideo)
                    ),
                HStackPanel().Margin(10)
                    .Children(
                        PzText("Text"),
                        PzTextBox(Text).Width(300),
                        PzText("Decrypted Text"),
                        PzTextBox(DeText).Width(300).IsReadOnly(true)
                    ),
                HStackPanel().Margin(10)
                    .Children(
                        SukiButton("TestCrypto").RxClick(OnTestCrypto)
                    ),
                HStackPanel().Margin(10)
                    .Children(
                        SukiButton("Close Viode Window").OnClick(_ => PreviewManager.CloseVideoWindow())
                    )
            );
    }

    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return [
            OnOpenImage.WithLatestFrom(SelectedFile)
                .Where(t => !string.IsNullOrEmpty(t.Second))
                .Select(t => t.Second)
                .Subscribe(PreviewManager.DevOpenImage),
            OnOpenVideo.WithLatestFrom(SelectedFile)
                .Where(t => !string.IsNullOrEmpty(t.Second))
                .Select(t => t.Second)
                .Subscribe(PreviewManager.DevOpenVideo),
            OnSelectFile.Select(_ => SelectFile())
                .Concat()
                .WhereNotEmpty(true)
                .Subscribe(SelectedFile),

            OnTestCrypto.WithLatestFrom(Text)
                .Select(t => t.Second)
                .WhereNotEmpty(true)
                .Select(TestCrypto)
                .Subscribe(DeText)
            ];
    }

    private readonly BehaviorSubject<string> SelectedFile = new(string.Empty);
    private readonly BehaviorSubject<string> Text = new(string.Empty);
    private readonly BehaviorSubject<string> DeText = new(string.Empty);

    private readonly Subject<RoutedEventArgs> OnOpenImage = new();
    private readonly Subject<RoutedEventArgs> OnOpenVideo = new();
    private readonly Subject<RoutedEventArgs> OnSelectFile = new();
    private readonly Subject<RoutedEventArgs> OnTestCrypto = new();

    private async Task<string?> SelectFile()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            return files[0].Path.LocalPath;
        }

        return null;
    }
    private string TestCrypto(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var crypto = PZCrypto.Create(Constants.Version, PZCrypto.CreateKey("123456"), 64);
        Span<byte> buffer = new byte[65536];
        var encLength = crypto.Encrypt(bytes, buffer);
        var enc = buffer[..encLength];

        Span<byte> dec = buffer[encLength..];
        var decLength = crypto.Decrypt(enc, dec);
        dec = buffer[encLength..(encLength + decLength)];
        return Encoding.UTF8.GetString(dec);
    }
}
