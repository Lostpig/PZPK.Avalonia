using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Material.Icons;
using PZPK.Core;
using PZPK.Core.Extract;
using PZPK.Desktop.Common;
using PZPK.Desktop.Previews;
using SukiUI.Content;
using SukiUI.Controls;
using System.Collections;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace PZPK.Desktop.Main.Explorer;

using static PZPK.Desktop.Common.ControlHelpers;

public class ExplorerPanel : PZComponentBase
{
    private Border BuildPackageDetail()
    {
        var pkg = Model.Package;

        return new Border()
            .Padding(20, 0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Child(
                new DockPanel().LastChildFill(false).Children(
                    HStackPanel().Spacing(10).Children(
                        SukiButton(() => LOC.PZPK.MetaData).OnClick(_ => ShowHeaderRaw()),
                        SukiButton(() => LOC.PZPK.Detail).OnClick(_ => ShowDetailRaw()),
                        SukiButton(() => LOC.PZPK.ExtractAll).OnClick(_ => ExtractAll()),
#if DEBUG
                        SukiButton(() => "Test Extract", "Accent").OnClick(_ => Model.DebugExtract())
#endif
                    ),
                    SukiButton(() => LOC.Base.Close, "Danger").OnClick(_ => Model.ClosePackage())
                        .Dock(Dock.Right)
                )
            );
    }

    private StackPanel DirStackFuncTemplete(PZFolder folder)
    {
        if (Index == null) return new StackPanel();
        var suki = App.Instance.Suki;

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

        var btn = new Border().Padding(5);
        if (folder.Id == Index.Root.Id)
        {
            btn.Child(MaterialIcon(MaterialIconKind.Package, 24));
        }
        else
        {
            btn.Child(PzText(folder.Name).VerticalAlignment(VerticalAlignment.Center));
        }

        btn.PointerReleased += (_, _) => Current.OnNext(folder);
        btn.PointerEntered += (_, _) => btn.Background = suki.GetSukiColor("SukiStrongBackground");
        btn.PointerExited += (_, _) => btn.Background = suki.GetSukiColor("SukiBackground");

        return HStackPanel().Children(btn, createArrow());
    }
    private ContextMenu BuildItemMenu()
    {
        return new ContextMenu()
            .Items(
                new MenuItem().Header(LOC.PZPK.Extract).OnClick(OnItemExtract),
                new MenuItem().Header(LOC.PZPK.Property).OnClick(OnItemProperty)
            );
    }

    protected override Control Build()
    {
        var dirStack = Current.Select(fo => {
                if (fo != null && Index != null) return Index.GetFolderResolveStack(fo);
                else return [];
            });
        var items = Current.Select(fo => {
                if (fo != null && Index != null)
                {
                    return Index.GetItems(fo, false).Sorted(NaturalPZItemComparer.Instance);
                }
                else
                {
                    return [];
                }
            });

        return Grid(null, "Auto, 50, 1*")
            .Children(
                new GlassCard().Row(0)
                    .Margin(10)
                    .Content(
                        Grid("Auto, 1*", null)
                            .Children(
                                MaterialIcon(MaterialIconKind.File, 48).Col(0),
                                BuildPackageDetail().Col(1)
                            )
                    ),
                new Border().Row(1)
                    .Margin(18, 0)
                    .CornerRadius(4)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Background(() => Suki.GetSukiColor("SukiGlassCardBackground"))
                    .Child(
                        new ItemsControl()
                            .ItemsSource(dirStack)
                            .ItemsPanel(HStackPanel())
                            .ItemTemplate<PZFolder, ItemsControl>(DirStackFuncTemplete)
                    ),
                new ListBox().Row(2)
                    .SelectionMode(SelectionMode.Multiple)
                    .ItemTemplate(new PZItemTemplate(BuildItemMenu()))
                    .ItemsSource(items)
                    .SelectedItems(SelectedItems)
                    .OnDoubleTapped(OnItemDoubleTap)
            );
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return 
        [
            Model.Package.Subscribe(p =>
            {
                if (p != null) Current.OnNext(p.Index.Root);
                else Current.OnNext(null);
            })
        ];
    }

    private static ExplorerModel Model => ExplorerModel.Instance;
    private static PackageIndex? Index => Model.Package.Value?.Index;
    private readonly BehaviorSubject<PZFolder?> Current = new(null);
    private readonly BehaviorSubject<IList> SelectedItems = new(new ArrayList());

    private void OnItemDoubleTap(TappedEventArgs e)
    {
        if (e.Source is Control ctrl)
        {
            if (ctrl.DataContext is PZFile file)
            {
                PreviewManager.PreviewFile(file);
            }
            else if (ctrl.DataContext is PZFolder folder)
            {
                Current.OnNext(folder);
            }
        }
    }

    private async void OnItemExtract(RoutedEventArgs e)
    {
        if (SelectedItems.Value is null) return;
        if (SelectedItems.Value.Count == 0) return;

        if (SelectedItems.Value.Count == 1)
        {
            var item = SelectedItems.Value[0];
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
            foreach (var selected in SelectedItems.Value)
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
    private async void ExtractAll()
    {
        if (Model.Package.Value != null)
        {
            ExtractFolder(Model.Package.Value.Index.Root);
        }
    }

    private void OnItemProperty(RoutedEventArgs e)
    {
        if (Model.Package.Value is null) return;

        if (e.Source is Control c && c.DataContext is IPZItem item)
        {
            if (item is PZFolder fo)
            {
                var files = Model.Package.Value.Index.GetFiles(fo, true);
                var size = files.Sum(f => f.Size);
                item = new ViewFolder(fo, files.Count, size);
            }

            var opt = PZDialog.AlertOptions(LOC.PZPK.Property, new ItemDialogContent(item));
            Model.Dialog.ShowDialog(opt);
        }
    }

    private static void ShowHeaderRaw()
    {
        var header = Model.Package.Value?.Header;
        if (header is null) return;

        List<LabelValuePair> pairs = 
        [
            new(LOC.PZPK.Version, header.Version.ToString()),
            new(LOC.PZPK.FileType, header.Type.ToString()),
            new(LOC.PZPK.Sign, Convert.ToHexString(header.Sign)),
            new(LOC.PZPK.PasswordChcek, Convert.ToHexString(header.PasswordCheck)),
            new(LOC.PZPK.CreateTime, header.CreateTime.ToString()),
            new(LOC.Base.Size, header.FileSize.ToString()),
            new(LOC.PZPK.BlockSize, header.BlockSize.ToString()),
            new("DetailOffset", header.DetailOffset.ToString()),
            new("DetailSize", header.DetailSize.ToString()),
            new("IndexOffset", header.IndexOffset.ToString()),
            new("IndexSize", header.IndexSize.ToString()),
        ];

        var content = new ItemDialogContent(pairs);
        var opt = PZDialog.AlertOptions(LOC.PZPK.Property, content);
        Model.Dialog.ShowDialog(opt);
    }
    private static void ShowDetailRaw()
    {
        var d = Model.Package.Value?.Detail;
        if (d is null) return;

        List<LabelValuePair> pairs =
        [
            new(LOC.PZPK.PackageName, d.Name),
            new(LOC.PZPK.Description, d.Description),
            new(LOC.PZPK.Tags, string.Join(',', d.Tags))
        ];

        var content = new ItemDialogContent(pairs);
        var opt = PZDialog.AlertOptions(LOC.PZPK.Property, content);
        Model.Dialog.ShowDialog(opt);
    }
}
