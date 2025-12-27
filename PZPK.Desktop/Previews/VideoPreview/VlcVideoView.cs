using Avalonia;
using Avalonia.Data;
using Avalonia.Platform;
using LibVLCSharp.Shared;
using System.Runtime.InteropServices;

namespace PZPK.Desktop.Previews.VideoPreview;

/// <summary>
/// Avalonia VideoView for Windows, Linux and Mac.
/// </summary>
public class VlcVideoView : NativeControlHost
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    private IPlatformHandle? _platformHandle = null;
    private MediaPlayer? _mediaPlayer = null;

    /// <summary>
    /// MediaPlayer Data Bound property
    /// </summary>
    /// <summary>
    /// Defines the <see cref="MediaPlayer"/> property.
    /// </summary>
    public static readonly DirectProperty<VlcVideoView, MediaPlayer?> MediaPlayerProperty =
        AvaloniaProperty.RegisterDirect<VlcVideoView, MediaPlayer?>(
            nameof(MediaPlayer),
            o => o.MediaPlayer,
            (o, v) => o.MediaPlayer = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Gets or sets the MediaPlayer that will be displayed.
    /// </summary>
    public MediaPlayer? MediaPlayer
    {
        get { return _mediaPlayer; }
        set
        {
            if (ReferenceEquals(_mediaPlayer, value))
            {
                return;
            }

            Detach();
            _mediaPlayer = value;
            Attach();
        }
    }

    /// <inheritdoc />
    public VlcVideoView()
    {
        Initialized += (_, _) => { Attach(); };
    }

    private void Attach()
    {
        if (_mediaPlayer == null || _platformHandle == null || !IsInitialized)
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _mediaPlayer.Hwnd = _platformHandle.Handle;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _mediaPlayer.XWindow = (uint)_platformHandle.Handle;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _mediaPlayer.NsObject = _platformHandle.Handle;
        }
    }

    private void Detach()
    {
        if (_mediaPlayer == null)
            return;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _mediaPlayer.Hwnd = IntPtr.Zero;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _mediaPlayer.XWindow = 0;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _mediaPlayer.NsObject = IntPtr.Zero;
        }
    }

    /// <inheritdoc />
    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        _platformHandle = base.CreateNativeControlCore(parent);

        if (_platformHandle.Handle != IntPtr.Zero && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            EnsureClipChildren(_platformHandle.Handle);
        }

        return _platformHandle;
    }

    private void EnsureClipChildren(IntPtr hwnd)
    {
        const int GWL_STYLE = -16;
        const uint WS_CLIPCHILDREN = 0x02000000;

        var style = GetWindowLong(hwnd, GWL_STYLE);
        var hasClipChildren = (style & WS_CLIPCHILDREN) != 0;

        if (!hasClipChildren)
        {
            var newStyle = style | WS_CLIPCHILDREN;
            SetWindowLong(hwnd, GWL_STYLE, newStyle);
        }
    }

    /// <inheritdoc />
    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        Detach();
        base.DestroyNativeControlCore(control);

        if (_platformHandle != null)
        {
            _platformHandle = null;
        }
    }
}