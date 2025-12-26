using SQLite;

namespace PZPK.Store.Tables;

[Table(PackageTable.TableName)]
public class Package
{
    [PrimaryKey, AutoIncrement]
    [Column(PackageTable.ColId)]
    public int Id { get; set; }

    [Column(PackageTable.ColName)]
    public string Name { get; set; } = "";

    [Column(PackageTable.ColPath)]
    public string Path { get; set; } = "";

    [Column(PackageTable.ColKey)]
    public byte[] Key { get; set; } = [];
}

internal static class PackageTable
{
    public const string TableName = "t_package";
    public const string ColId = "id";
    public const string ColName = "name";
    public const string ColPath = "path";
    public const string ColKey = "key";
}

