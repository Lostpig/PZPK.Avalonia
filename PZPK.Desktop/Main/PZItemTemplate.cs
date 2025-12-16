using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using Material.Icons;
using PZPK.Core;
using PZPK.Desktop.Common;

namespace PZPK.Desktop.Main;
using static PZPK.Desktop.Common.ControlHelpers;

public class PZItemTemplate(ContextMenu? menu = null) : IDataTemplate
{
    public ContextMenu? Menu { get; set; } = menu;

    public bool Match(object? data)
    {
        return data is IPZFile || data is IPZFolder;
    }
    public Control Build(object? data)
    {
        string size = "";
        string name;
        MaterialIconKind icon;
        if (data is IPZFile file)
        {
            name = file.Name;
            size = Utility.ComputeFileSize(file.Size);
            icon = MaterialIconKind.FileDocument;
        }
        else if (data is IPZFolder folder)
        {
            name = folder.Name;
            icon = MaterialIconKind.Folder;
        }
        else
        {
            name = "Error item";
            icon = MaterialIconKind.Error;
        }

        var content = Grid("40, 1*, 120")
            .Background(Brushes.Transparent)
            .Children(
                MaterialIcon(icon).Col(0).VerticalAlignment(VerticalAlignment.Center),
                PzText(name).Col(1).VerticalAlignment(VerticalAlignment.Center),
                PzText(size).Col(2).VerticalAlignment(VerticalAlignment.Center)
            );
        if (Menu != null)
        {
            content.ContextMenu = Menu;
        }

        return content;
    }
}
