using SQLite;

namespace PZPK.Store.Tables;

[Table(CategoryTable.TableName)]
public class Category
{
    [PrimaryKey, AutoIncrement]
    [Column(CategoryTable.ColId)]
    public int Id { get; set; }

    [Column(CategoryTable.ColName)]
    public string Name { get; set; } = "";

    [Column(CategoryTable.ColCode)]
    public string Code { get; set; } = "";

    [Column(CategoryTable.ColShortCode)]
    public string ShortCode { get; set; } = "";
}

internal static class CategoryTable
{
    public const string TableName = "t_category";
    public const string ColId = "id";
    public const string ColName = "name";
    public const string ColCode = "code";
    public const string ColShortCode = "short_code";
}
