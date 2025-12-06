using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
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

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

public class IndexPanel(CreatorModel vm) : ComponentBase<CreatorModel>(vm)
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
        var index = ViewModel?.Index;

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
    override protected object Build(CreatorModel vm)
    {
        if (vm is null) throw new InvalidOperationException("ViewModel cannot be null");
        var suki = App.Instance.Suki;

        return Grid(null, "50, 1*, 40")
            .Children(
                new Border().Row(0)
                    .Margin(18, 0)
                    .CornerRadius(4)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Background(() => suki.GetSukiColor("SukiGlassCardBackground"))
                    .Child(
                        HStackPanel().Children(() => BuildFolderStack())
                    ),
                new ListBox().Row(1)
                    .SelectionMode(SelectionMode.Multiple)
                    .ItemTemplate(new PZItemTemplate())
                    .ItemsSource(() => Items)
                    .OnDoubleTapped(OnItemDoubleTap),
                new DockPanel().Row(2)
                    .Classes("buttons")
                    .Children(
                        SukiButton("Add Files").OnClick(_ => AddFiles()),
                        SukiButton("Add Folder").OnClick(_ => AddFolder()),
                        SukiButton("New Folder").OnClick(_ => NewFolder()),
                        SukiButton("Resort").OnClick(_ => Resort()),
                        SukiButton("Clear").OnClick(_ => Clear()),
                        HStackPanel()
                            .HorizontalAlignment(HorizontalAlignment.Right)
                            .Dock(Dock.Right)
                            .Children(
                                SukiButton("Next", "Flat").Margin(5, 0).OnClick(_ => Next())
                            )
                    )
            );
    }

    private readonly IndexCreator Index = vm.Index;
    private PZIndexFolder Current = vm.Index.Root;
    private List<IPZItem> Items = [];

    private void OnItemDoubleTap(TappedEventArgs e)
    {
        if (e.Source is Control ctrl)
        {
            // if (ctrl.DataContext is PZIndexFile file)
            // {
            //     ImagePreviewManager.OpenImage(file);
            // }
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

    private void NewFolder()
    {
        App.Instance.MainWindow.DialogManager.CreateDialog()
            .WithTitle("New Folder")
            .WithContent(new NameDialogContent())
            .WithActionButton("OK", (d) =>
            {
                if (d.Content is NameDialogContent ndc)
                {
                    var text = ndc.GetResult();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        ViewModel?.Index?.AddFolder(text.Trim(), Current);
                        UpdateList();
                        d.Dismiss();
                    }
                }
            })
            .WithActionButton("Cancel", _ => { }, true)
            .TryShow();
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
                Toast.Error(ex.Message);
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
                Toast.Error(ex.Message);
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

        string dx = 'D' + w.ToString();
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
        var ok = await Dialog.Confirm("Sure to clear all files?");

        if (ok)
        {
            Current = Index.Root;
            Index.Clear();
            UpdateList();
        }
    }

    private void Next()
    {
        ViewModel?.NextStep();
    }
}
