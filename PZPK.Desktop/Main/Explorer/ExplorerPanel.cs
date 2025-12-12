using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Material.Icons;
using PZPK.Core;
using PZPK.Desktop.Common;
using PZPK.Desktop.ImagePreview;
using SukiUI.Content;
using SukiUI.Controls;
using System.Linq;

namespace PZPK.Desktop.Main.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class ExplorerPanel : PZComponentBase
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
                        PzText(() => $"{LOC.PZPK.PackageName}: {PackageName}").Margin(0, 0, 0, 8),
                        PzText(() => $"{LOC.PZPK.Description}: {Description}").Margin(0, 0, 0, 8),
                        PzText(() => $"{LOC.PZPK.Tags}: {Tags}").Margin(0, 0, 0, 8),
                        PzText(() => FormatInfomation())
                    )
            );
    }
    private StackPanel BuildPackageOperators()
    {
        return VStackPanel()
            .VerticalAlignment(VerticalAlignment.Center)
            .Children(
                SukiButton(LOC.PZPK.ExtractAll).Margin(0, 0, 0, 10).OnClick(_ => OnExtractAll()),
                SukiButton(LOC.Base.Close, "Outlined", "Accent").OnClick(_ => ClosePackage())
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
                new MenuItem().Header(LOC.PZPK.Extract).OnClick(OnItemExtract),
                new MenuItem().Header(LOC.PZPK.Property).OnClick(OnItemProperty)
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
                ItemContainer.Row(2)
            );
    }

    private readonly ExplorerModel Model;
    private string Description = "";
    private string PackageName = "";
    private string Tags = "";
    private MaterialIconKind TypeIcon = MaterialIconKind.File;
    private PZFolder? Current;
    private List<IPZItem> Items = [];
    private readonly ListBox ItemContainer;

    public ExplorerPanel(ExplorerModel model): base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        Model.OnPackageOpened += OnPackageOpened;

        ItemContainer = new ListBox()
                    .SelectionMode(SelectionMode.Multiple)
                    .ItemTemplate(new PZItemTemplate(BuildItemMenu()))
                    .ItemsSource(() => Items)
                    .OnDoubleTapped(OnItemDoubleTap);

        Initialize();
    }
    private string FormatInfomation()
    {
        if (Model.Package is null) return "";

        var header = Model.Package.Header;
        string version = header.Version.ToString();
        string size = Utility.ComputeFileSize(header.FileSize);
        string blockSize = Utility.ComputeFileSize(header.BlockSize);
        string createTime = header.CreateTime.ToString("yyyy-MM-dd HH:mm:ss");

        return $"{LOC.PZPK.Version}: {version} | {LOC.Base.Size}: {size} | {LOC.PZPK.BlockSize}: {blockSize} | {LOC.PZPK.CreateTime}: {createTime}";
    }
    private void OnPackageOpened()
    {
        var package = Model.Package;

        Description = package?.Detail.Description ?? "";
        PackageName = package?.Detail.Name ?? "";
        Tags = string.Join(',', package?.Detail.Tags ?? []);
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
        if (ItemContainer.SelectedItems is null) return;
        if (ItemContainer.SelectedItems.Count == 0) return;

        if (ItemContainer.SelectedItems.Count == 1)
        {
            var item = ItemContainer.SelectedItems[0];
            if (item is PZFile file)
            {
                ExtractFile(file);
            }
            else if (item is PZFolder folder)
            {
                ExtractFolder(folder);
            }
        }
        else
        {
            List<IPZItem> items = [];
            foreach (var selected in ItemContainer.SelectedItems)
            {
                if (selected is IPZItem item)
                {
                    items.Add(item);
                }
            }

            ExtractBatch(items);
        }
    }
    private async void ExtractFile(PZFile file)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var dest = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = LOC.PZPK.ExtractFile,
            SuggestedFileName = file.Name,
            DefaultExtension = file.Extension,
        });

        if (dest is not null)
        {
            Model.ExtractFile(file, dest.Path.LocalPath);
        }
    }
    private async void ExtractFolder(PZFolder folder)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var dest = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = LOC.PZPK.SelectDirectory,
        });
        if (dest is not null && dest.Count > 0)
        {
            Model.ExtractFolder(folder, dest[0].Path.LocalPath);
        }
    }
    private async void ExtractBatch(List<IPZItem> items)
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var dest = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = LOC.PZPK.SelectDirectory,
        });
        if (dest is not null && dest.Count > 0)
        {
            Model.ExtractBatch(items, dest[0].Path.LocalPath);
        }
    }
    private void OnItemProperty(RoutedEventArgs e)
    {
        if (Model.Package is null) return;

        if (e.Source is Control c && c.DataContext is IPZItem item)
        {
            if (item is PZFolder fo)
            {
                var files = Model.Package.Index.GetFiles(fo, true);
                var size = files.Sum(f => f.Size);
                item = new ViewFolder(fo, files.Count, size);
            }

            Model.Dialog.ShowContentDialog(LOC.PZPK.Property, new ItemDialogContent(item));
        }
    }

    private async void OnExtractAll()
    {
        if (Model.Package is null) return;
        ExtractFolder(Model.Package.Root);
    }
}
