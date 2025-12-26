using PZPK.Store.Tables;
using SQLite;

namespace PZPK.Store;

public class Store
{
    public static Store Open(string dbPath, string password)
    {
        var sqlite = SqlHandler.Open(dbPath, password);
        return new Store(sqlite);
    }
    public static Store Create(string dbPath, string password)
    {
        var sqlite = SqlHandler.Create(dbPath, password);

        string basepath = Path.GetDirectoryName(dbPath)!;
        sqlite.DB.Insert(new Variant() { Key = VariantKeys.BasePath, Value = basepath });

        return new Store(sqlite);
    }

    private readonly SqlHandler _sqlite;
    private SQLiteConnection DB => _sqlite.DB;
    public string BasePath {  get; private set; }

    private Store(SqlHandler sqlite)
    {
        _sqlite = sqlite;

        var basepathVar = DB.Find<Variant>(VariantKeys.BasePath);
        BasePath = basepathVar.Value;
    }

    public void SetBasePath(string path)
    {
        BasePath = path; 
        DB.Update(new Variant() { Key = VariantKeys.BasePath, Value = BasePath }); 
    }

    public void AddTag(string title, int categoryId)
    {
        var newTag = new Tag()
        {
            Title = title,
            CategoryId = categoryId
        };
        DB.Insert(newTag);
    }
    public void AddCategory(string name, string code, string shortCode)
    {
        var newCategory = new Category()
        {
            Name = name,
            Code = code,
            ShortCode = shortCode
        };
        DB.Insert(newCategory);
    }
    public void AddPackage(string name, string path, byte[] key) 
    {
        var newPackage = new Package()
        {
            Name= name,
            Path = path,
            Key = key
        };
        DB.Insert(newPackage); 
    }
    public void AddPackageTag(int packageId, int tagId)
    {
        var newItem = new PackageTag()
        {
            PackageId = packageId,
            TagId = tagId
        };
        DB.Insert(newItem);
    }

    public void DeleteTag(int tagId)
    {
        var ptDelQuery = $"DELETE FROM {PackageTagTable.TableName} WHERE {PackageTagTable.ColTagId} = {tagId}";
        var tagDelQuery = $"DELETE FROM {TagTable.TableName} WHERE {TagTable.ColId} = {tagId}";

        DB.RunInTransaction(() =>
        {
            DB.Execute(ptDelQuery);
            DB.Execute(tagDelQuery);
        });
    }
    public void DeleteCategory(int cateId)
    {
        var Tags = DB.Table<Tag>().Where(t => t.CategoryId == cateId).ToList();
        List<string> commands = [];
        foreach (var tag in Tags)
        {
            var ptDelQuery = $"DELETE FROM {PackageTagTable.TableName} WHERE {PackageTagTable.ColTagId} = {tag.Id}";
            var tagDelQuery = $"DELETE FROM {TagTable.TableName} WHERE {TagTable.ColId} = {tag.Id}";
            commands.Add(ptDelQuery);
            commands.Add(tagDelQuery);
        }

        var cataDelQuery = $"DELETE FROM {CategoryTable.TableName} WHERE {CategoryTable.ColId} = {cateId}";

        DB.RunInTransaction(() => { 
            foreach (var command in commands)
            {
                DB.Execute(command);
            }

            DB.Execute(cataDelQuery);
        });
    }
    public void DeletePackage(int pkgId)
    {
        var ptDelQuery = $"DELETE FROM {PackageTagTable.TableName} WHERE {PackageTagTable.ColPackageId} = {pkgId}";
        var pkgDelQuery = $"DELETE FROM {PackageTable.TableName} WHERE {PackageTable.ColId} = {pkgId}";

        DB.RunInTransaction(() =>
        {
            DB.Execute(ptDelQuery);
            DB.Execute(pkgDelQuery);
        });
    }
    public void DeletePackageTag(int pkgId, int tagId)
    {
        var ptDelQuery = $"DELETE FROM {PackageTagTable.TableName} " +
            $"WHERE {PackageTagTable.ColTagId} = {tagId} AND {PackageTagTable.ColPackageId} = {pkgId}";

        DB.Execute(ptDelQuery);
    }

    public List<Tag> GetTags()
    {
        return DB.Table<Tag>().ToList();
    }
    public List<Tag> GetTags(int cateId, string title = "")
    {
        var query = DB.Table<Tag>().Where(t => t.CategoryId == cateId);

        title = title.Trim();
        if (!string.IsNullOrEmpty(title))
        {
            query = query.Where(t => t.Title.Contains(title)); 
        }

        return query.ToList();
    }
    public List<Package> FindPackages(IEnumerable<int> tagIds, string name = "")
    {
        string query = $"SELECT pkg.* FROM {PackageTable.TableName} pkg ";
        string where = "WHERE 1 = 1 ";

        if (tagIds.Any())
        {
            query += $"LEFT JOIN {PackageTagTable.TableName} pt ON pt.{PackageTagTable.ColPackageId} = pkg.{PackageTable.ColId} ";
            where += $"AND pt.{PackageTagTable.ColTagId} IN ({string.Join(",", tagIds)}) ";
        }
        if (!string.IsNullOrWhiteSpace(name))
        {
            where += $"AND pkg.{PackageTable.ColName} LIKE '%{name.Trim()}%' ";
        }

        return DB.Query<Package>(query + where);
    }
    public List<Category> GetCategories()
    {
        return DB.Table<Category>().ToList();
    }
}
