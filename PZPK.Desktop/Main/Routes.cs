using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using Material.Icons;
using PZPK.Desktop.Modules.Creator;
using PZPK.Desktop.Modules.Explorer;
using PZPK.Desktop.Modules.Notebook;
using System;
using System.Collections.Generic;

namespace PZPK.Desktop.Main;

internal class Routes
{
    static public readonly PageRecord[] Pages = [
        new PageRecord("Explorer", MaterialIconKind.Explore, typeof(ExplorerPage)),
        new PageRecord("Creator", MaterialIconKind.Create, typeof(CreatorPage)),
        new PageRecord("Notebook", MaterialIconKind.Book, typeof(NoteBookPage)),
#if DEBUG
        new PageRecord("Dev", MaterialIconKind.DeveloperBoard, typeof(Modules.Dev.DevPage)),
#endif
    ];
}

internal record PageRecord(string PageName, MaterialIconKind Icon, Type PageType);
internal class PageLocator : IDataTemplate
{
    private readonly Dictionary<Type, ComponentBase> _views = [];

    public Control Build(object? param)
    {
        if (param is PageRecord pr)
        {
            if (_views.TryGetValue(pr.PageType, out var page))
            {
                return page;
            }

            if (pr.PageType.IsAssignableTo(typeof(ComponentBase)))
            {
                page = Activator.CreateInstance(pr.PageType) as ComponentBase;

                if (page is not null)
                {
                    _views.Add(pr.PageType, page);
                    return page;
                }
            }
        }

        return new TextBlock() { Text = "create page param error." };
    }

    public bool Match(object? data) => data is PageRecord;
}