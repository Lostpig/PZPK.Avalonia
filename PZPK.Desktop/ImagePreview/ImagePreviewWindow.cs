using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PZPK.Core;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;

namespace PZPK.Desktop.ImagePreview;
using static PZPK.Desktop.Common.ControlHelpers;

struct MouseState
{
    public MouseState() { }
    public double LastX = 0;
    public double LastY = 0;
    public bool Active = false;
}
public class ImagePreviewWindow : PZWindowBase
{
    private static PreviewModel Model => PreviewModel.Instance;
    private readonly Image ImageRef;
    private readonly OperateBar OperateBarRef;
    private readonly InfoBar InfoBarRef;
    private readonly ScrollViewer ScrollRef;
    private bool FileChangedFlag = false;
    private PZFile? File;
    private List<PZFile> Files = [];

    public ImagePreviewWindow() : base()
    {
        ImageRef = new Image()
            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Stretch)
            .ZIndex(0);

        RenderOptions.SetBitmapInterpolationMode(ImageRef, BitmapInterpolationMode.HighQuality);

        OperateBarRef = new OperateBar().ZIndex(1).Row(0);
        InfoBarRef = new InfoBar().ZIndex(1).Row(2);
        ScrollRef = new ScrollViewer()
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Hidden)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Hidden)
            .RowSpan(3)
            .OnPointerPressed(OnMouseDown)
            .OnPointerReleased(OnMouseUp)
            .OnPointerMoved(OnMouseMove)
            .OnPointerWheelChanged(OnMouseWheel)
            .OnLayoutUpdated(OnScrollLayoutUpdated)
            .Content(
                ImageRef
            );

        Content = Grid(null, "40,*,40")
            .Children(
                ScrollRef,
                OperateBarRef,
                InfoBarRef
            );

        InitializeOperators();
    }

    private List<IDisposable> _subscriptions = [];
    private void InitializeOperators()
    {
        _subscriptions.AddRange(
            Observable.FromEventPattern<SizeChangedEventArgs>(
                h => ScrollRef.SizeChanged += h,
                h => ScrollRef.SizeChanged -= h
            ).Select(e => e.EventArgs.NewSize).Subscribe(Model.ContainerSize.OnNext),
            Model.Current.Subscribe(LoadImage),
            Observable.When(
                Model.Size.And(Model.Scale).Then((size, scale) => (size, scale))
            ).Throttle(TimeSpan.FromMilliseconds(250)).Subscribe(UpdateImageScale)
        );
    }

    protected void OnScrollLayoutUpdated(EventArgs e)
    {
        if (!FileChangedFlag) return;

        var xOffset = ScrollRef.ScrollBarMaximum.X > 0 ? ScrollRef.ScrollBarMaximum.X / 2 : 0;
        ScrollRef.SetCurrentValue(ScrollViewer.OffsetProperty, new Vector(xOffset, 0));

        FileChangedFlag = false;
    }

    public void OpenImage(PZFile file, List<PZFile> files)
    {
        Files = files;
        var index = files.IndexOf(file);
        index = index < 0 ? 0 : index;

        Model.Current.OnNext(index);
        Model.FileName.OnNext(File?.Name ?? "");
        Model.Total.OnNext(Files.Count);
    }
    public void DevOpenImage(string imgFile)
    {
        var f = System.IO.File.OpenRead(imgFile);
        var bytes = new byte[f.Length];
        f.ReadExactly(bytes);
        var memStream = new MemoryStream(bytes);
        memStream.Seek(0, SeekOrigin.Begin);

        var bitmap = new Bitmap(memStream);

        ImageRef.Source = bitmap;

        File = null;
        Files = [];
        Model.Current.OnNext(0);
        Model.FileName.OnNext("Dev Test");
        Model.Total.OnNext(Files.Count);
    }

    private async void LoadImage(int index)
    {
        var newFile = Files[index];
        if (newFile == File) return;

        File = newFile;
        try
        {
            var bytes = PZPKPackage.Current!.Package.ExtractFile(File);
            using var bitmapStream = new MemoryStream(bytes);
            bitmapStream.Seek(0, SeekOrigin.Begin);
            Bitmap bitmap = new(bitmapStream);

            ImageRef.Source = bitmap;
            Model.Size.OnNext(bitmap.PixelSize);

            switch (Model.Lock.Value)
            {
                case LockMode.FitWidth:
                    Model.FitToWidth();
                    break;
                case LockMode.FitHeight:
                    Model.FitToHeight();
                    break;
                case LockMode.None:
                    Model.Scale.OnNext(1);
                    break;
                default:
                    break;
            }

            FileChangedFlag = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            Toast.Error(ex.Message);
            ImageRef.Source = null;
        }
    }
    private void UpdateImageScale((PixelSize, double) sns)
    {
        var (size, scale) = sns;
        double renderWidth = size.Width * scale;
        double renderHeight = size.Height * scale;

        ImageRef.Width = renderWidth;
        ImageRef.Height = renderHeight;
    }

    private MouseState MState = new();
    private void OnMouseDown(PointerPressedEventArgs e)
    {
        var p = e.GetCurrentPoint(ScrollRef);
        if (p.Properties.IsLeftButtonPressed)
        {
            // Implement mouse down logic for dragging the image
        }
        else if (p.Properties.IsRightButtonPressed)
        {
            MState.Active = true;
            MState.LastX = p.Position.X;
            MState.LastY = p.Position.Y;
        }
        else if (p.Properties.IsXButton1Pressed)
        {
            Model.Current.Reducer(i => i - 1);
        }
        else if (p.Properties.IsXButton2Pressed)
        {
            Model.Current.Reducer(i => i + 1);
        }
    }
    private void OnMouseUp(PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Right)
        {
            MState.Active = false;
        }
    }
    private void OnMouseMove(PointerEventArgs e)
    {
        if (!MState.Active) return;

        var p = e.GetCurrentPoint(ScrollRef);
        Point pos = p.Position;

        var movedX = p.Position.X - MState.LastX;
        var movedY = p.Position.Y - MState.LastY;

        Vector movedOffset = new(ScrollRef.Offset.X - movedX, ScrollRef.Offset.Y - movedY);
        ScrollRef.SetCurrentValue(ScrollViewer.OffsetProperty, movedOffset);

        MState.LastX = pos.X;
        MState.LastY = pos.Y;
    }
    private void OnMouseWheel(PointerWheelEventArgs e)
    {
        e.Handled = true;

        if (e.Delta.Y > 0)
        {
            Model.Scale.Reducer(s => s + 0.1);
        }
        else if (e.Delta.Y < 0)
        {
            Model.Scale.Reducer(s => s - 0.1);
        }
    }
}
