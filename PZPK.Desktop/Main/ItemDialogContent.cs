using PZPK.Core;
using PZPK.Desktop.Common;

namespace PZPK.Desktop.Main;
using static Common.ControlHelpers;

internal record ViewFolder(IPZFolder Folder, int Files, long Size) : IPZItem
{
    public string Name => Folder.Name;
    public int Id => Folder.Id;
    public int Pid => Folder.Pid;
}

internal record LabelValuePair(string Label, string Value);

internal class ItemDialogContent : ContentControl
{
    public ItemDialogContent(IPZItem item)
    {
        var pairs = item switch
        {
            PZFile file => PZFileContent(file),
            PZIndexFile ifile => PZIndexFileContent(ifile),
            ViewFolder folder => FolderContent(folder),
            _ => []
        };

        RenderContent(pairs);
    }
    public ItemDialogContent(IEnumerable<LabelValuePair> pairs)
    {
        RenderContent(pairs);
    }

    private void RenderContent(IEnumerable<LabelValuePair> pairs)
    {
        var content = VStackPanel(Avalonia.Layout.HorizontalAlignment.Stretch);
        foreach (var pair in pairs)
        {
            content.Children.Add(ContentItem(pair));
        }

        Content = content;
    }
    private static DockPanel ContentItem(LabelValuePair item)
    {
        return new DockPanel()
            .Margin(0, 0, 0, 10)
            .Children(
                PzText(item.Label).FontWeight(Avalonia.Media.FontWeight.Bold),
                PzText(item.Value)
                    .Dock(Dock.Right)
                    .MaxWidth(200)
                    .TextWrapping(Avalonia.Media.TextWrapping.Wrap)
                    .HorizontalAlignment(Avalonia.Layout.HorizontalAlignment.Right)
            );
    }

    public static List<LabelValuePair> PZItemBaseContent(IPZItem item)
    {
        return
        [
            new("Id", item.Id.ToString()),
            new("Pid", item.Pid.ToString()),
            new("Name", item.Name),
        ];
    }
    public static List<LabelValuePair> PZFileContent(PZFile file)
    {
        return 
        [
            .. PZItemBaseContent(file),
            new("Extension", file.Extension),
            new("Size", Utility.ComputeFileSize(file.Size)),
            new("OriginSize", Utility.ComputeFileSize(file.OriginSize)),
        ];
    }
    public static List<LabelValuePair> PZIndexFileContent(PZIndexFile file)
    {
        return
        [
            .. PZItemBaseContent(file),
            new("Extension", file.Extension),
            new("Source", file.Source),
            new("Size", Utility.ComputeFileSize(file.Size))
        ];
    }
    public static List<LabelValuePair> FolderContent(ViewFolder folder)
    {
        return
        [
            .. PZItemBaseContent(folder),
            new("Files", folder.Files.ToString()),
            new("Total Size", Utility.ComputeFileSize(folder.Size))
        ];
    }
}
