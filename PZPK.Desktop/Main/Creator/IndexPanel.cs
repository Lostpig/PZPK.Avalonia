using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Material.Icons;
using PZ.RxAvalonia;
using PZPK.Core;
using PZPK.Core.Packing;
using PZPK.Desktop.Common;
using SukiUI.Content;
using SukiUI.Dialogs;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                new MenuItem().Header("Rename").OnClick(OnItemRename),
                new MenuItem().Header("Delete").OnClick(OnItemDelete),
                new MenuItem().Header("Property").OnClick(OnItemProperty)
            );
    }
    private StackPanel DirStackFuncTemplete(PZIndexFolder folder)
    {
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
        btn.PointerEntered += (_, _) => btn.Background = highlightBg;
        btn.PointerExited += (_, _) => btn.Background = normalBg;

        return HStackPanel().Children(btn, createArrow());
    }

    override protected Control Build()
    {
        var suki = App.Instance.Suki;
        var items = Observable.When(
                    Current.And(Changed.Where(x => x)
                ).Then((fo, _) => fo))
                .Select(fo => Index.GetItems(fo, false).Sorted(NaturalPZItemComparer.Instance));
        var dirStack = Current.Select(fo => Index.GetFolderResolveStack(fo).Reverse());

        return Grid(null, "50, 1*, 40")
            .Children(
                new Border().Row(0)
                    .Margin(18, 0)
                    .CornerRadius(4)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Background(() => suki.GetSukiColor("SukiGlassCardBackground"))
                    .Child(
                        new ItemsControl()
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
                        SukiButton("Add File").OnClick(_ => AddFiles()),
                        SukiButton("Add Folder").OnClick(_ => AddFolder()),
                        SukiButton("New Folder").OnClick(_ => NewFolder()),
                        SukiButton("Resort", "Accent").OnClick(_ => Resort()),
                        SukiButton("Clear", "Flat", "Warning").OnClick(_ => Clear()),
                        HStackPanel()
                            .HorizontalAlignment(HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                            .Children(
                                SukiButton("Next", "Flat").Margin(5, 0).OnClick(_ => Model.NextStep())
                            )
                    )
            );
    }
    protected override void OnCreated()
    {
        base.OnCreated();
        Model.Completed.Subscribe(_ => Current.OnNext(Index.Root));
    }

    private static CreatorModel Model => CreatorModel.Instance;
    private static IndexCreator Index => CreatorModel.Instance.Index;
    private readonly BehaviorSubject<PZIndexFolder> Current = new(Index.Root);
    private readonly Subject<bool> Changed = new();

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
        var name = await ShowNameDialog("Create folder");

        if (!string.IsNullOrEmpty(name))
        {
            Index.AddFolder(name, Current.Value);
            Changed.OnNext(true);
        }
    }
    private async void AddFiles()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Files",
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
                Model.Toast.Error(ex.Message);
                Logger.Instance.Log(ex.Message);
            }
            finally
            {
                Changed.OnNext(added);
            }
        }
    }
    private async void AddFolder()
    {
        TopLevel topLevel = TopLevel.GetTopLevel(this)!;
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder",
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
                Model.Toast.Error(ex.Message);
                Logger.Instance.Log(ex.Message);
            }
            finally
            {
                Changed.OnNext(true);
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

        Changed.OnNext(true);
    }
    private async void Clear()
    {
        var ok = await Model.Dialog.DeleteConfirm("Sure to clear all files?");

        if (ok)
        {
            Index.Clear();
            Current.OnNext(Index.Root);
        }
    }

    private async void OnItemRename(RoutedEventArgs e)
    {
        if (e.Source is Control c)
        {
            if (c.DataContext is PZIndexFile f)
            {
                var newName = await ShowNameDialog("Rename file", f.Name);
                if (!string.IsNullOrEmpty(newName) && newName != f.Name)
                {
                    Index.RenameFile(f, newName);
                    Changed.OnNext(true);
                }
            }
            else if (c.DataContext is PZIndexFolder fo)
            {
                var newName = await ShowNameDialog("Rename folder", fo.Name);
                if (!string.IsNullOrEmpty(newName) && newName != fo.Name)
                {
                    Index.RenameFolder(fo, newName);
                    Changed.OnNext(true);
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
                Changed.OnNext(true);
            }
            else if (c.DataContext is PZIndexFolder fo)
            {
                Index.RemoveFolder(fo);
                Changed.OnNext(true);
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

            Model.Dialog.ShowContentDialog("Property", new ItemDialogContent(item));
        }
    }

    private static async Task<string?> ShowNameDialog(string title, string originName = "")
    {
        var content = new NameDialogContent(originName);
        var builder = Model.Dialog.Manager.CreateDialog()
            .WithTitle(title)
            .WithContent(content);

        var completion = new TaskCompletionSource<string?>();
        builder.WithActionButton("OK", (d) =>
        {
            var text = content.GetResult();
            if (!string.IsNullOrEmpty(text))
            {
                completion.SetResult(text.Trim());
                d.Dismiss();
            }
        });
        builder.WithActionButton("Cancel", (d) =>
        {
            completion.SetResult(null);
            d.Dismiss();
        });
        builder.TryShow();

        return await completion.Task;
    }
}
