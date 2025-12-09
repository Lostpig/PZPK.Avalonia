using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Material.Icons;
using PZPK.Core;
using PZPK.Core.Packing;
using PZPK.Desktop.Common;
using PZPK.Desktop.Global;
using SukiUI.Content;
using SukiUI.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PZPK.Desktop.Main.Creator;

using static Common.ControlHelpers;

public class IndexPanel : ComponentBase
{
    protected override StyleGroup? BuildStyles()
    {
        return [
                new Style<DockPanel>(s => s.Class("buttons").Child().Is<Button>())
                    .Margin(5, 0)
            ];
    }
    private List<Control> BuildFolderStack()
    {
        List<Control> controls = [];
        var current = Current;
        var index = Model.Index;

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

        if (index is not null && current is not null)
        {
            var dirStack = index.GetFolderResolveStack(current);

            var rootIcon = new Border().Padding(5)
                .Child(MaterialIcon(MaterialIconKind.Package, 24));
            rootIcon.PointerReleased += (_, _) => EnterDirectory(index.Root);
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
                new MenuItem().Header("Rename").OnClick(OnItemRename),
                new MenuItem().Header("Delete").OnClick(OnItemDelete),
                new MenuItem().Header("Property").OnClick(OnItemProperty)
            );
    }

    override protected object Build()
    {
        var suki = App.Instance.Suki;

        return Grid(null, "50, 1*, 40")
            .Children(
                new Border().Row(0)
                    .Margin(18, 0)
                    .CornerRadius(4)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Background(() => suki.GetSukiColor("SukiGlassCardBackground"))
                    .Child(
                        HStackPanel().Children(BuildFolderStack)
                    ),
                new ListBox().Row(1)
                    .SelectionMode(SelectionMode.Multiple)
                    .ItemTemplate(new PZItemTemplate(BuildItemMenu()))
                    .ItemsSource(() => Items)
                    .OnDoubleTapped(OnItemDoubleTap),
                new DockPanel().Row(2)
                    .Classes("buttons")
                    .Children(
                        SukiButton("Add Files").OnClick(_ => AddFiles()),
                        SukiButton("Add Folder").OnClick(_ => AddFolder()),
                        SukiButton("New Folder").OnClick(_ => NewFolder()),
                        SukiButton("Resort", "Accent").OnClick(_ => Resort()),
                        SukiButton("Clear", "Flat", "Warning").OnClick(_ => Clear()),
                        HStackPanel()
                            .HorizontalAlignment(HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                            .Children(
                                SukiButton("Next", "Flat").Margin(5, 0).OnClick(_ => Next())
                            )
                    )
            );
    }

    public IndexPanel(CreatorModel model) : base(ViewInitializationStrategy.Lazy)
    {
        Model = model;
        Index = model.Index;
        Current = Index.Root;

        Model.OnStepChanged += OnStepChanged;

        Initialize();
    }

    private readonly CreatorModel Model;
    private readonly IndexCreator Index;
    private PZIndexFolder Current;
    private List<IPZItem> Items = [];

    private void OnStepChanged()
    {
        if (Model.Step != 1) return;

        Current = Index.Root;
        UpdateList();
    }

    private void OnItemDoubleTap(TappedEventArgs e)
    {
        if (e.Source is Control ctrl)
        {
            if (ctrl.DataContext is PZIndexFolder folder)
            {
                EnterDirectory(folder);
            }
        }
    }
    private void EnterDirectory(PZIndexFolder folder)
    {
        if (folder == Current) return;

        Current = folder;
        UpdateList();
    }
    private void UpdateList()
    {
        var files = Index.GetFiles(Current, false);
        var folders = Index.GetFolders(Current, false);
        files.Sort(NaturalPZItemComparer.Instance);
        folders.Sort(NaturalPZItemComparer.Instance);

        List<IPZItem> items = [.. folders, .. files];
        Items = items;

        StateHasChanged();
    }

    private async void NewFolder()
    {
        var name = await ShowNameDialog("Create folder");

        if (!string.IsNullOrEmpty(name))
        {
            Index.AddFolder(name, Current);
            UpdateList();
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
                    Index.AddFile(f.Path.LocalPath, f.Name, Current);
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
                if (added)
                {
                    UpdateList();
                }
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

        if (folders.Count >= 1)
        {
            var folder = folders[0].Path.LocalPath;
            try
            {
                Debug.WriteLine(folder);
                var di = new DirectoryInfo(folder);
                ScanAndAddFolder(di, Current);
            }
            catch (Exception ex)
            {
                Model.Toast.Error(ex.Message);
                Logger.Instance.Log(ex.Message);
            }
            finally
            {
                UpdateList();
            }
        }
    }
    private void ScanAndAddFolder(DirectoryInfo di, PZIndexFolder parent)
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
        var files = Index.GetFiles(Current, false);
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

        UpdateList();
    }
    private async void Clear()
    {
        var ok = await Model.Dialog.DeleteConfirm("Sure to clear all files?");

        if (ok)
        {
            Current = Index.Root;
            Index.Clear();
            UpdateList();
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
                    UpdateList();
                }
            }
            else if (c.DataContext is PZIndexFolder fo)
            {
                var newName = await ShowNameDialog("Rename folder", fo.Name);
                if (!string.IsNullOrEmpty(newName) && newName != fo.Name)
                {
                    Index.RenameFolder(fo, newName);
                    UpdateList();
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
                UpdateList();
            }
            else if (c.DataContext is PZIndexFolder fo)
            {
                Index.RemoveFolder(fo);
                UpdateList();
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

    private async Task<string?> ShowNameDialog(string title, string originName = "")
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

    private void Next()
    {
        Model.NextStep();
    }
}
