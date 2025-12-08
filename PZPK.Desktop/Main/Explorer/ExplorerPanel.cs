using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Platform.Storage;
using Material.Icons;
using PZPK.Core;
using PZPK.Desktop.Common;
using PZPK.Desktop.ImagePreview;
using SukiUI.Content;
using SukiUI.Controls;
using System.Collections.Generic;
using System.Diagnostics;

namespace PZPK.Desktop.Main.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class ExplorerPanel : ComponentBase
{
    private Border BuildPackageDetail()
    {
        var suki = App.Instance.Suki;

        return new Border()
            .Margin(12, 0, 32, 0)
            .Padding(20, 0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .BorderThickness(1, 0)
            .BorderBrush(() => suki.GetSukiColor("SukiLowText"))
            .Child(
                VStackPanel()
                    .Children(
                        PzText(() => "PackageName: " + PackageName).Margin(0, 0, 0, 8),
                        PzText(() => "Description: " + Description).Margin(0, 0, 0, 8),
                        PzText(() => "Tags: " + Tags).Margin(0, 0, 0, 8),
                        PzText(() => Info)
                    )
            );
    }
    private StackPanel BuildPackageOperators()
    {
        return VStackPanel()
            .VerticalAlignment(VerticalAlignment.Center)
            .Children(
                SukiButton("Extract All").Margin(0, 0, 0, 10).OnClick(_ => OnExtractAll()),
                SukiButton("Close", "Outlined", "Accent").OnClick(_ => ClosePackage())
            );
    }
    private List<Control> BuildFolderStack()
    {
        List<Control> controls = [];
        var current = Current;
        var package = Model.Package;

        var normalBg = App.Instance.Suki.GetSukiColor("SukiBackground");
        var highlightBg = App.Instance.Suki.GetSukiColor("SukiStrongBackground");

        static PathIcon createArrow() => new()
        {
            Data = Icons.ChevronRight,
            Height = 12,
            Width = 12,
            Margin = new Thickness(10, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Classes = { "Flippable" }
        };

        if (package is not null && current is not null)
        {
            var dirStack = package.Index.GetFolderResolveStack(current);

            var rootIcon = new Border().Padding(5)
                .Child(MaterialIcon(MaterialIconKind.Package, 24));
            rootIcon.PointerReleased += (_, _) => EnterDirectory(package.Root);
            rootIcon.PointerEntered += (_, _) => rootIcon.Background = highlightBg;
            rootIcon.PointerExited += (_, _) => rootIcon.Background = normalBg;
            controls.Add(rootIcon);
            controls.Add(createArrow());

            while (dirStack.Count > 0)
            {
                var f = dirStack.Pop();

                var folderBtn = new Border().Padding(5)
                    .Child(PzText(f.Name).VerticalAlignment(VerticalAlignment.Center));
                folderBtn.PointerReleased += (_, _) => EnterDirectory(f);
                folderBtn.PointerEntered += (_, _) => folderBtn.Background = highlightBg;
                folderBtn.PointerExited += (_, _) => folderBtn.Background = normalBg;
                controls.Add(folderBtn);
                controls.Add(createArrow());
            }
        }

        return controls;
    }
    private ContextMenu BuildItemMenu()
    {
        return new ContextMenu()
            .Items(
                new MenuItem().Header("Preview"),
                new MenuItem().Header("Extract").OnClick(OnItemExtract)
            );
    }

    protected override object Build()
    {
        var suki = App.Instance.Suki;

        return Grid(null, "Auto, 50, 1*")
            .Children(
                new GlassCard().Row(0)
                    .Margin(10)
                    .Content(
                        Grid("Auto, 1*, Auto", null)
                            .Children(
                                MaterialIcon(TypeIcon, 48).Col(0),
                                BuildPackageDetail().Col(1),
                                BuildPackageOperators().Col(2)
                            )
                    ),
                new Border().Row(1)
                    .Margin(18, 0)
                    .CornerRadius(4)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Background(() => suki.GetSukiColor("SukiGlassCardBackground"))
                    .Child(
                        HStackPanel().Children(() => BuildFolderStack())
                    ),
                new ListBox().Row(2)
                    .SelectionMode(SelectionMode.Multiple)
                    .ItemTemplate(new PZItemTemplate(BuildItemMenu()))
                    .ItemsSource(() => Items)
                    .OnDoubleTapped(OnItemDoubleTap)
            );
    }

    private readonly ExplorerModel Model;
    private string Description = "";
    private string PackageName = "";
    private string Tags = "";
    private string Info = "";
    private MaterialIconKind TypeIcon = MaterialIconKind.File;
    private PZFolder? Current;
    private List<IPZItem> Items = [];

    public ExplorerPanel(ExplorerModel model): base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        Model.OnPackageOpened += OnPackageOpened;

        Initialize();
    }
    private void OnPackageOpened()
    {
        var package = Model.Package;

        Description = package?.Detail.Description ?? "";
        PackageName = package?.Detail.Name ?? "";
        Tags = string.Join(',', package?.Detail.Tags ?? []);
        Info = package is not null ? $"FileType: {package.Header.Type} | Version: {package.Header.Version} | Size: {Utility.ComputeFileSize(package.Header.FileSize)}" : "";
        TypeIcon = package?.Header.Type switch
        {
            PZType.Package => MaterialIconKind.Package,
            PZType.Note => MaterialIconKind.Archive,
            _ => MaterialIconKind.File
        };

        if (package is null)
        {
            Current = null;
            Items.Clear();
        }
        else
        {
            EnterDirectory(package.Root, true);
        }

        StateHasChanged();
    }
    private void ClosePackage()
    {
        Model.ClosePackage();
    }
    private void EnterDirectory(PZFolder folder, bool forceUpdate = false)
    {
        if (folder == Current && !forceUpdate) return;

        if (Model.Package is not null)
        {
            var index = Model.Package.Index;
            var files = index.GetFiles(folder, false);
            var folders = index.GetFolders(folder, false);
            files.Sort(NaturalPZItemComparer.Instance);
            folders.Sort(NaturalPZItemComparer.Instance);

            List<IPZItem> items = [.. folders, .. files];

            Current = folder;
            Items = items;

            StateHasChanged();
        }
    }
    private void OnItemDoubleTap(TappedEventArgs e)
    {
        if (e.Source is Control ctrl)
        {
            if (ctrl.DataContext is PZFile file)
            {
                ImagePreviewManager.OpenImage(file);
            }
            else if (ctrl.DataContext is PZFolder folder)
            {
                EnterDirectory(folder);
            }
        }
    }

    private async void OnItemExtract(RoutedEventArgs e)
    {
        Debug.WriteLine(e.ToString());

        if (e.Source is MenuItem mi)
        {
            if (mi.DataContext is PZFile file)
            {
                TopLevel topLevel = TopLevel.GetTopLevel(this)!;
                var dest = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Extract file",
                    SuggestedFileName = file.Name,
                    DefaultExtension = file.Extension,
                });

                if (dest is not null)
                {
                    Model.ExtractFile(file, dest.Path.LocalPath);
                }
            }
        }
    }
    private async void OnExtractAll()
    {
        Model.DebugExtract();
    }
}
