using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Material.Icons;
using PZPK.Core;
using PZPK.Core.Packing;
using PZPK.Desktop.Common;
using SukiUI.Content;
using SukiUI.Dialogs;
using System.Collections.Generic;

namespace PZPK.Desktop.Main.Creator;
using static Common.ControlHelpers;

internal class IndexPanel(CreatorModel vm) : ComponentBase<CreatorModel>(vm)
{
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
        var suki = App.Instance.Suki;

        return Grid(null, "50, 1*, 60")
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
                HStackPanel().Row(2)
                    .Children(
                        SukiButton("Add"),
                        SukiButton("Add Folder"),
                        SukiButton("New Folder").OnClick(_ => NewFolder()),
                        SukiButton("Resort"),
                        SukiButton("Clear"),
                        SukiButton("Next").HorizontalAlignment(HorizontalAlignment.Right)
                    )
            );
    }

    private IndexCreator Index = vm.Index;
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
            .WithContent(new NameDialogContent())
            .WithActionButton("OK", (d) =>
            {
                if (d.Content is NameDialogContent ndc)
                {
                    if (!string.IsNullOrWhiteSpace(ndc.Result))
                    {
                        ViewModel?.Index?.AddFolder(ndc.Result.Trim(), Current);
                        UpdateList();
                        d.Dismiss();
                    }
                }
            })
            .WithActionButton("Cancel", _ => { }, true)
            .TryShow();
    }
}
