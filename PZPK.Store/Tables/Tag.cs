using SQLite;

namespace PZPK.Store.Tables;

[Table(TagTable.TableName)]
public class Tag
{
    [PrimaryKey, AutoIncrement]
    [Column(TagTable.ColId)]
    public int Id { get; set; }

    [Column(TagTable.ColTitle)]
    public string Title { get; set; } = "";

    [Column(TagTable.ColCategoryId)]
    public int CategoryId { get; set; }
}

internal static class TagTable
{
    public const string TableName = "t_tag";
    public const string ColId = "id";
    public const string ColTitle = "title";
    public const string ColCategoryId = "category_id";
}