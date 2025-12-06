using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Material.Icons;
using PZPK.Core;
using PZPK.Desktop.Common;
using PZPK.Desktop.ImagePreview;
using PZPK.Desktop.Main;
using SukiUI.Content;
using SukiUI.Controls;
using System;
using System.Collections.Generic;

namespace PZPK.Desktop.Main.Explorer;
using static PZPK.Desktop.Common.ControlHelpers;

public class ExplorerPanel(ExplorerModel vm) : ComponentBase<ExplorerModel>(vm)
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
                SukiButton("Extract").Margin(0, 0, 0, 10),
                SukiButton("Close", "").OnClick(_ => ClosePackage())
            );
    }
    private List<Control> BuildFolderStack()
    {
        List<Control> controls = [];
        var current = Current;
        var model = ViewModel?.Model;

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

        if (model is not null && current is not null)
        {
            var dirStack = model.Package.Index.GetFolderResolveStack(current);

            var rootIcon = new Border().Padding(5)
                .Child(MaterialIcon(MaterialIconKind.Package, 24));
            rootIcon.PointerReleased += (_, _) => EnterDirectory(model.Root);
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
    protected override object Build(ExplorerModel vm)
    {
        if (vm is null) throw new InvalidOperationException("ViewModel cannot be null");
        var suki = App.Instance.Suki;
        vm.OnPackageOpened += OnPackageOpened;

        return Grid(null, "Auto, 50, 1*")
            .Children(
                new GlassCard().Row(0)
                    .Margin(10)
                    .Content(
                        Grid("100, 1*, 100", null)
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
                    .ItemTemplate(new PZItemTemplate())
                    .ItemsSource(() => Items)
                    .OnDoubleTapped(OnItemDoubleTap)
            );
    }

    private string Description = "";
    private string PackageName = "";
    private string Tags = "";
    private string Info = "";
    private MaterialIconKind TypeIcon = MaterialIconKind.File;
    private PZFolder? Current;
    private List<IPZItem> Items = [];

    private void OnPackageOpened()
    {
        var model = ViewModel?.Model;

        Description = model?.Detail.Description ?? "";
        PackageName = model?.Detail.Name ?? "";
        Tags = string.Join(',', model?.Detail.Tags ?? []);
        Info = model is not null ? $"FileType: {model.Header.Type} | Version: {model.Header.Version} | Size: {Utility.ComputeFileSize(model.Header.FileSize)}" : "";
        TypeIcon = model?.Header.Type switch
        {
            PZType.Package => MaterialIconKind.Package,
            PZType.Note => MaterialIconKind.Archive,
            _ => MaterialIconKind.File
        };

        if (model is null)
        {
            Current = null;
            Items.Clear();
        }
        else
        {
            EnterDirectory(model.Root, true);
        }

        StateHasChanged();
    }
    private void ClosePackage()
    {
        ViewModel?.ClosePackage();
    }
    private void EnterDirectory(PZFolder folder, bool forceUpdate = false)
    {
        if (folder == Current && !forceUpdate) return;

        if (ViewModel?.Model is not null)
        {
            var index = ViewModel.Model.Package.Index;
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
}
