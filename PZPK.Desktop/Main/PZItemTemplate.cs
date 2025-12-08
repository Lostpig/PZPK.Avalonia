using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Material.Icons;
using PZPK.Core;
using PZPK.Desktop.Common;
using Avalonia.Markup.Declarative;
using Avalonia.Media;

namespace PZPK.Desktop.Main;
using static PZPK.Desktop.Common.ControlHelpers;

public class PZItemTemplate : IDataTemplate
{
    public ContextMenu? Menu { get; set; }
    public PZItemTemplate (ContextMenu? menu = null)
    {
        Menu = menu;
    }

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
            .Classes("explorer-item")
            .Background(Brushes.Transparent)
            .Children(
                MaterialIcon(icon).Col(0),
                PzText(name).Col(1),
                PzText(size).Col(2)
            );
        if (Menu != null)
        {
            content.ContextMenu = Menu;
        }

        return content;
    }
}
