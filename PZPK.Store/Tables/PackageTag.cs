using SQLite;

namespace PZPK.Store.Tables;

[Table(PackageTagTable.TableName)]
public class PackageTag
{
    [Column(PackageTagTable.ColTagId), Indexed]
    public int TagId { get; set; }

    [Column(PackageTagTable.ColPackageId), Indexed]
    public int PackageId { get; set; }
}

internal static class PackageTagTable
{
    public const string TableName = "t_package_tag";
    public const string ColTagId = "tag_id";
    public const string ColPackageId = "package_id";
}
