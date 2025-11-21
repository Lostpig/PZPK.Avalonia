using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using PZPK.Core;
using PZPK.Core.Crypto;
using PZPK.Desktop.Modules.Explorer;
using PZPK.Desktop.Modules.ImagePreview;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace PZPK.Desktop.Modules.Dev;
using static PZPK.Desktop.Common.ControlHelpers;

internal class DevPage : ComponentBase
{
    protected override object Build()
    {
        return VStackPanel()
            .Children(
                HStackPanel().Margin(10)
                    .Children(
                        TextedBlock("File"),
                        PzTextBox(() => SelectedFile, v => SelectedFile = v).Width(400).IsReadOnly(true),
                        PzButton("Select File").OnClick(_ => OnSelectFile()),
                        PzButton("OpenAsImage").OnClick(_ => OnOpenAsImage())
                    ),
                HStackPanel().Margin(10)
                    .Children(
                        TextedBlock("Text"),
                        PzTextBox(() => Text, v => Text = v).Width(300),
                        TextedBlock("Decrypted Text"),
                        PzTextBox(() => DeText, v => DeText = v).Width(300).IsReadOnly(true)
                    ),
                HStackPanel().Margin(10)
                    .Children(
                        PzButton("TestCrypto").OnClick(_ => TestCrypto())
                    )
            );
    }

    private string SelectedFile = string.Empty;
    private string Text = string.Empty;
    private string DeText = string.Empty;
    private async void OnSelectFile()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });

        if (files.Count >= 1)
        {
            SelectedFile = files[0].Path.LocalPath;
            StateHasChanged();
        }
    }
    private void OnOpenAsImage()
    {
        if (string.IsNullOrEmpty(SelectedFile)) return;

        ImagePreviewManager.DevOpenImage(SelectedFile);
    }
    private void TestCrypto()
    {
        if (string.IsNullOrEmpty(Text))
        {
            DeText = "";
            StateHasChanged();
            return;
        }

        var bytes = Encoding.UTF8.GetBytes(Text);
        var crypto = PZCrypto.Create(Constants.Version, PZCrypto.CreateKey("123456"), 64);
        Span<byte> buffer = new byte[65536];
        var encLength = crypto.Encrypt(bytes, buffer);
        var enc = buffer[..encLength];

        Span<byte> dec = buffer[encLength..];
        var decLength = crypto.Decrypt(enc, dec);
        dec = buffer[encLength..(encLength + decLength)];
        DeText = Encoding.UTF8.GetString(dec);
        StateHasChanged();

    }
}
