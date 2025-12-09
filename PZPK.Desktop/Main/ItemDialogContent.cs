using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using PZPK.Core;
using PZPK.Desktop.Common;
using System.Linq;

namespace PZPK.Desktop.Main;
using static Common.ControlHelpers;

internal record ViewFolder(IPZFolder Folder, int Files, long Size) : IPZItem
{
    public string Name => Folder.Name;
    public int Id => Folder.Id;
    public int Pid => Folder.Pid;
}

internal class ItemDialogContent : ContentControl
{
    public ItemDialogContent(IPZItem item)
    {
        var content = VStackPanel(Avalonia.Layout.HorizontalAlignment.Stretch);
        AddBaseContent(content, item);
        if (item is PZFile file)
        {
            BuildFileContent(content, file);
        }
        else if (item is PZIndexFile indexFile)
        {
            BuildIndexFileContent(content, indexFile);
        }
        else if (item is ViewFolder folder)
        {
            BuildFolderContent(content, folder);
        }

        Content = content;
    }

    private DockPanel ContentItem(string label, string value)
    {
        return new DockPanel()
            .Margin(0, 0, 0, 10)
            .Children(
                PzText(label).FontWeight(Avalonia.Media.FontWeight.Bold),
                PzText(value)
                    .Dock(Dock.Right)
                    .MaxWidth(200)
                    .TextWrapping(Avalonia.Media.TextWrapping.Wrap)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
            );
    }
    private void AddBaseContent(StackPanel content, IPZItem item)
    {

#if DEBUG
        content.Children.Add(ContentItem("Id", item.Id.ToString()));
        content.Children.Add(ContentItem("Pid", item.Pid.ToString()));
#endif
        content.Children.Add(ContentItem("Name", item.Name));
    }
    private void BuildFileContent(StackPanel content, PZFile file)
    {
        content.Children(
                ContentItem("Extension", file.Extension),
                ContentItem("Size", Utility.ComputeFileSize(file.Size)),
                ContentItem("OriginSize", Utility.ComputeFileSize(file.OriginSize))
            );
    }
    private void BuildIndexFileContent(StackPanel content, PZIndexFile file)
    {
        content.Children(
                ContentItem("Extension", file.Extension),
                ContentItem("Source", file.Source),
                ContentItem("Size", Utility.ComputeFileSize(file.Size))
            );
    }
    private void BuildFolderContent(StackPanel content, ViewFolder folder)
    {
        content.Children(
            ContentItem("Files", folder.Files.ToString()),
            ContentItem("Total Size", Utility.ComputeFileSize(folder.Size))
        );
    }
}
