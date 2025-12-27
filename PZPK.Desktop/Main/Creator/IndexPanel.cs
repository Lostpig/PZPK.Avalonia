using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Material.Icons;
using PZPK.Core;
using PZPK.Core.Packing;
using PZPK.Desktop.Common;
using SukiUI.Content;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace PZPK.Desktop.Main.Creator;

using static Common.ControlHelpers;

public class IndexPanel : PZComponentBase
{
    protected override StyleGroup? BuildStyles()
    {
        return [
                new Style<DockPanel>(s => s.Class("buttons").Child().Is<Button>())
                    .Margin(5, 0)
            ];
    }
    private ContextMenu BuildItemMenu()
    {
        return new ContextMenu()
            .Items(
                new MenuItem().Header(() => LOC.PZPK.Rename).OnClick(OnItemRename),
                new MenuItem().Header(() => LOC.Base.Delete).OnClick(OnItemDelete),
                new MenuItem().Header(() => LOC.PZPK.Property).OnClick(OnItemProperty)
            );
    }
    private StackPanel DirStackFuncTemplete(PZIndexFolder folder)
    {
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
        btn.PointerEntered += (_, _) => btn.Background = Suki.GetSukiColor("SukiStrongBackground");
        btn.PointerExited += (_, _) => btn.Background = Suki.GetSukiColor("SukiBackground");

        return HStackPanel().Children(btn, createArrow());
    }
    override protected Control Build()
    {
        var items = Current.CombineLatest(Changed)
                .Select(t => Index.GetItems(t.First, false).Sorted(NaturalPZItemComparer.Instance));
        var dirStack = Current.Select(fo => Index.GetFolderResolveStack(fo));

        return Grid(null, "50, 1*, 40")
            .Children(
                new Border().Row(0)
                    .Margin(18, 0)
                    .CornerRadius(4)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Background(() => Suki.GetSukiColor("SukiGlassCardBackground"))
                    .Child(
                        new ItemsControl()
                            .ItemsPanel(HStackPanel())
                            .ItemsSource(dirStack)
                            .ItemTemplate<PZIndexFolder, ItemsControl>(DirStackFuncTemplete)
                    ),
                new ListBox().Row(1)
                    .SelectionMode(SelectionMode.Multiple)
                    .ItemTemplate(new PZItemTemplate(BuildItemMenu()))
                    .ItemsSource(items)
                    .OnDoubleTapped(OnItemDoubleTap),
                new DockPanel().Row(2)
                    .Classes("buttons")
                    .Children(
                        SukiButton(() => LOC.PZPK.AddFile).OnClick(_ => AddFiles()),
                        SukiButton(() => LOC.PZPK.AddDirectory).OnClick(_ => AddFolder()),
                        SukiButton(() => LOC.PZPK.CreateFolder).OnClick(_ => NewFolder()),
                        SukiButton(() => LOC.PZPK.ResortFiles, "Accent").OnClick(_ => Resort()),
                        SukiButton(() => LOC.PZPK.Clear, "Flat", "Warning").OnClick(_ => Clear()),
                        HStackPanel()
                            .HorizontalAlignment(HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                            .Children(
                                SukiButton(() => LOC.Base.Next, "Flat").Margin(5, 0).OnClick(_ => Model.NextStep())
                            )
                    )
            );
    }
    protected override IEnumerable<IDisposable> WhenActivate()
    {
        return [
            Model.Step.Subscribe(s => {
                if (s.from == 4 && s.current == 1)
                {
                    Current.OnNext(Index.Root);
                    Changed.OnNext(Unit.Default);
                }
            })
        ];
    }

    private static CreatorModel Model => CreatorModel.Instance;
    private static IndexCreator Index => CreatorModel.Instance.Index;
    private readonly BehaviorSubject<PZIndexFolder> Current = new(Index.Root);
    private readonly Subject<Unit> Changed = new();

    private void OnItemDoubleTap(TappedEventArgs e)
    {
        if (e.Source is Control ctrl)
        {
            if (ctrl.DataContext is PZIndexFolder folder)
            {
                Current.OnNext(folder);
            }
        }
    }
    private async void NewFolder()
    {
        var name = await ShowNameDialog(LOC.PZPK.CreateFolder);

        if (!string.IsNullOrEmpty(name))
        {
            try
            {
                Index.AddFolder(name, Current.Value);
                Changed.OnNext(Unit.Default);
            }
            catch (Exception ex)
            {
                Model.OnErrorCatch(ex);
            }
        }
    }
    private async void AddFiles()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = LOC.PZPK.SelectFiles,
            AllowMultiple = true
        });

        if (files is not null)
        {
            bool added = false;
            try
            {
                foreach (var f in files)
                {
                    Index.AddFile(f.Path.LocalPath, f.Name, Current.Value);
                    added = true;
                }
            }
            catch (Exception ex)
            {
                Model.OnErrorCatch(ex);
            }
            finally
            {
                if (added) Changed.OnNext(Unit.Default);
            }
        }
    }
    private async void AddFolder()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = LOC.PZPK.SelectDirectory,
            AllowMultiple = false
        });

        if (folders is not null && folders.Count >= 1)
        {
            var folder = folders[0].Path.LocalPath;
            try
            {
                Debug.WriteLine(folder);
                var di = new DirectoryInfo(folder);
                ScanAndAddFolder(di, Current.Value);
            }
            catch (Exception ex)
            {
                Model.OnErrorCatch(ex);
            }
            finally
            {
                Changed.OnNext(Unit.Default);
            }
        }
    }
    private static void ScanAndAddFolder(DirectoryInfo di, PZIndexFolder parent)
    {
        PZIndexFolder current = Index.AddFolder(di.Name, parent);

        var dirs = di.GetDirectories();
        var files = di.GetFiles();

        foreach (var file in files)
        {
            Index.AddFile(file, current);
        }
        foreach (var dir in dirs)
        {
            ScanAndAddFolder(dir, current);
        }
    }

    private void Resort()
    {
        var files = Index.GetFiles(Current.Value, false);
        files.Sort(NaturalPZItemComparer.Instance);

        int w = 2;
        while (Math.Pow(10, w) < files.Count)
        {
            w++;
        }

        string dx = 'D' + (w + 1).ToString();
        int i = 0;
        foreach (var file in files)
        {
            i++;
            string idx = i.ToString(dx);
            Index.RenameFile(file, idx + file.Extension);
        }

        Changed.OnNext(Unit.Default);
    }
    private async void Clear()
    {
        var opt = PZDialog.ConfirmOptions(LOC.Base.Warning, LOC.Message.SureToClear);
        var ok = await Model.Dialog.ShowDialog(opt);

        if (ok)
        {
            Index.Clear();
            Current.OnNext(Index.Root);
        }
    }

    private async void OnItemRename(RoutedEventArgs e)
    {
        if (e.Source is Control c && c.DataContext is IPZItem item)
        {
            var newName = await ShowNameDialog(LOC.PZPK.Rename, item.Name);
            if (!string.IsNullOrEmpty(newName) && newName != item.Name)
            {
                try
                {
                    if (item is PZIndexFile file) Index.RenameFile(file, newName);
                    else if (item is PZIndexFolder folder) Index.RenameFolder(folder, newName);
                    Changed.OnNext(Unit.Default);
                }
                catch (Exception ex)
                {
                    Model.OnErrorCatch(ex);
                }
            }
        }
    }
    private async void OnItemDelete(RoutedEventArgs e)
    {
        if (e.Source is Control c)
        {
            if (c.DataContext is PZIndexFile f)
            {
                Index.RemoveFile(f);
                Changed.OnNext(Unit.Default);
            }
            else if (c.DataContext is PZIndexFolder fo)
            {
                Index.RemoveFolder(fo);
                Changed.OnNext(Unit.Default);
            }
        }
    }
    private void OnItemProperty(RoutedEventArgs e)
    {
        if (e.Source is Control c && c.DataContext is IPZItem item)
        {
            if (item is PZIndexFolder fo)
            {
                var files = Index.GetFiles(fo, true);
                var size = files.Sum(f => f.Size);
                item = new ViewFolder(fo, files.Count, size);
            }

            var opt = PZDialog.AlertOptions(LOC.PZPK.Property, new ItemDialogContent(item));
            Model.Dialog.ShowDialog(opt);
        }
    }

    private static async Task<string?> ShowNameDialog(string title, string originName = "")
    {
        var content = new NameDialogContent(originName);
        var opt = PZDialog.ConfirmOptions(title, content);
        var ok = await Model.Dialog.ShowDialog(opt);

        var result = ok ? content.GetResult() : null;
        return result;
    }
}
