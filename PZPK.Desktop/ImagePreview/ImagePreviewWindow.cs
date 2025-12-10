using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PZPK.Core;
using PZPK.Desktop.Common;
using PZPK.Desktop.Global;
using SukiUI.Controls;
using SukiUI.Toasts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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
    private readonly PreviewModel Model;
    private readonly Image ImageRef;
    private readonly OperateBar OperateBarRef;
    private readonly InfoBar InfoBarRef;
    private readonly ScrollViewer ScrollRef;
    private bool FileChangedFlag = false;
    private int Index = 0;
    private PZFile? File;
    private List<PZFile> Files = [];

    public ImagePreviewWindow() : base()
    {
        Model = new();

        ImageRef = new Image()
            .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Stretch)
            .VerticalAlignment(Avalonia.Layout.VerticalAlignment.Stretch)
            .ZIndex(0);

        RenderOptions.SetBitmapInterpolationMode(ImageRef, BitmapInterpolationMode.HighQuality);

        OperateBarRef = new OperateBar(Model).ZIndex(1).Row(0);
        InfoBarRef = new InfoBar(Model).ZIndex(1).Row(2);
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

        InitOperates();
    }

    public void OpenImage(PZFile file, List<PZFile> files)
    {
        Index = files.IndexOf(file);
        Index = Index < 0 ? 0 : Index;
        Files = files;

        LoadImage();
        UpdateChildBars();
    }
    public void DevOpenImage(string imgFile)
    {
        Index = 0;
        Files = [];

        var f = System.IO.File.OpenRead(imgFile);
        var bytes = new byte[f.Length];
        f.ReadExactly(bytes);
        var memStream = new MemoryStream(bytes);
        memStream.Seek(0, SeekOrigin.Begin);

        var bitmap = new Bitmap(memStream);

        ImageRef.Source = bitmap;
        Model.OriginSize = bitmap.PixelSize;
        Model.Scale = 1;
    }

    protected void OnScrollLayoutUpdated(EventArgs e)
    {
        if (!FileChangedFlag) return;

        var xOffset = ScrollRef.ScrollBarMaximum.X > 0 ? ScrollRef.ScrollBarMaximum.X / 2 : 0;
        ScrollRef.SetCurrentValue(ScrollViewer.OffsetProperty, new Vector(xOffset, 0));

        FileChangedFlag = false;
    }

    private void InitOperates()
    {
        OperateBarRef.ImageChange += ChangeImage;
        OperateBarRef.ScaleChange += ChangeScale;
        OperateBarRef.ToOriginSize += ToOriginSize;
        OperateBarRef.FitToWidth += FitToWidth;
        OperateBarRef.FitToHeight += FitToHeight;
        OperateBarRef.ToggleFullScreen += ToggleFullScreen;
    }
    private void ChangeImage(int diff)
    {
        int after = Index + diff;
        if (after < 0) after = 0;
        if (after >= Files.Count) after = Files.Count - 1;

        if (Index != after)
        {
            Index = after;
            LoadImage();
            UpdateChildBars();
        }
    }
    private void ChangeScale(double changeValue)
    {
        double newScale = Model.Scale + changeValue;
        if (newScale < 0.1) newScale = 0.1;
        else if (newScale > 5) newScale = 5;

        Model.Scale = newScale;
        UpdateImageScale();
        UpdateChildBars();
    }
    private void FitToHeight()
    {
        if (ImageRef.Source == null) return;
        double viewerHeight = ScrollRef.Bounds.Height;
        double imageHeight = ImageRef.Source.Size.Height;
        Model.Scale = viewerHeight / imageHeight;
        UpdateImageScale();
        UpdateChildBars();
    }
    private void FitToWidth()
    {
        if (ImageRef.Source == null) return;
        double viewerWidth = ScrollRef.Bounds.Width;
        double imageWidth = ImageRef.Source.Size.Width;
        Model.Scale = viewerWidth / imageWidth;
        UpdateImageScale();
        UpdateChildBars();
    }
    private void ToOriginSize()
    {
        Model.Scale = 1;
        UpdateImageScale();
        UpdateChildBars();
    }

    private async void LoadImage()
    {
        var newFile = Files[Index];
        if (newFile == File) return;

        File = newFile;
        try
        {
            var bytes = PZPKPackage.Current!.Package.ExtractFile(File);
            using var bitmapStream = new MemoryStream(bytes);
            bitmapStream.Seek(0, SeekOrigin.Begin);
            Bitmap bitmap = new(bitmapStream);

            ImageRef.Source = bitmap;
            Model.OriginSize = bitmap.PixelSize;

            switch (Model.Lock)
            {
                case LockMode.FitWidth:
                    FitToWidth();
                    break;
                case LockMode.FitHeight:
                    FitToHeight();
                    break;
                case LockMode.None:
                    ToOriginSize();
                    break;
                default:
                    UpdateImageScale();
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
    private void UpdateImageScale()
    {
        double renderWidth = Model.OriginSize.Width * Model.Scale;
        double renderHeight = Model.OriginSize.Height * Model.Scale;

        ImageRef.Width = renderWidth;
        ImageRef.Height = renderHeight;

        // Model.RenderedSize = new(renderWidth, renderHeight);
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
            ChangeImage(-1);
        }
        else if (p.Properties.IsXButton2Pressed)
        {
            ChangeImage(1);
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
            ChangeScale(0.1);
        }
        else if (e.Delta.Y < 0)
        {
            ChangeScale(-0.1);
        }
    }

    private void UpdateChildBars()
    {
        Model.FileName = File?.Name ?? "";
        Model.Total = Files.Count;
        Model.Current = Index + 1;
        Model.SizeText = $"{Model.OriginSize.Width} x {Model.OriginSize.Height}";
        Model.FileSizeText = Utility.ComputeFileSize(File?.OriginSize ?? 0);

        InfoBarRef.UpdateState();
        OperateBarRef.UpdateState();
    }
}
