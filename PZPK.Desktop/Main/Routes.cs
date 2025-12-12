using Avalonia.Controls.Templates;
using Material.Icons;
using PZPK.Desktop.Main.Creator;
using PZPK.Desktop.Main.Explorer;
using PZPK.Desktop.Main.Notebook;
using PZPK.Desktop.Main.Setting;

namespace PZPK.Desktop.Main;

internal class Routes
{
    static public readonly PageRecord[] Pages = [
        new PageRecord(() => LOC.Base.Explorer, MaterialIconKind.Explore, typeof(ExplorerPage)),
        new PageRecord(() => LOC.Base.Creator, MaterialIconKind.Create, typeof(CreatorPage)),
        new PageRecord(() => LOC.Base.Notebook, MaterialIconKind.Book, typeof(NoteBookPage)),
        new PageRecord(() => LOC.Base.Setting, MaterialIconKind.Settings, typeof(SettingPage)),
#if DEBUG
        new PageRecord(() => "Dev", MaterialIconKind.DeveloperBoard, typeof(Dev.DevPage)),
#endif
    ];
}

internal class PageRecord(Func<string> PageNameGetter, MaterialIconKind icon, Type pageType)
{
    private readonly Func<string> _getter = PageNameGetter;
    public MaterialIconKind Icon { get; init; } = icon;
    public Type PageType { get; init; } = pageType;
    public string PageName => _getter();

}
internal class PageLocator : IDataTemplate
{
    private static PageLocator? _instance;
    public static PageLocator Instance
    {
        get
        {
            _instance ??= new PageLocator();
            return _instance;
        }
    }
    private PageLocator() { }

    private readonly Dictionary<Type, PZComponentBase> _views = [];
    public void Reset()
    {
        _views.Clear(); 
    }

    public Control Build(object? param)
    {
        if (param is PageRecord pr)
        {
            if (_views.TryGetValue(pr.PageType, out var page))
            {
                return page;
            }

            if (pr.PageType.IsAssignableTo(typeof(PZComponentBase)))
            {
                page = Activator.CreateInstance(pr.PageType) as PZComponentBase;

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