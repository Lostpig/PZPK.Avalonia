using Avalonia.Controls.Templates;
using Material.Icons;
using PZPK.Desktop.Main.Creator;
using PZPK.Desktop.Main.Explorer;
using PZPK.Desktop.Main.Notebook;
using System;
using System.Collections.Generic;

namespace PZPK.Desktop.Main;

internal class Routes
{
    static public readonly PageRecord[] Pages = [
        new PageRecord(() => LOC.Explorer, MaterialIconKind.Explore, typeof(ExplorerPage)),
        new PageRecord(() => LOC.Creator, MaterialIconKind.Create, typeof(CreatorPage)),
        new PageRecord(() => LOC.Notebook, MaterialIconKind.Book, typeof(NoteBookPage)),
#if DEBUG
        new PageRecord(() => "Dev", MaterialIconKind.DeveloperBoard, typeof(Dev.DevPage)),
#endif
    ];
}

internal class PageRecord
{
    public PageRecord(Func<string> PageNameGetter, MaterialIconKind icon, Type pageType)
    {
        Icon = icon;
        PageType = pageType;
        _getter = PageNameGetter;
    }

    private Func<string> _getter;
    public MaterialIconKind Icon { get; init; }
    public Type PageType { get; init; }
    public string PageName => _getter();

}
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