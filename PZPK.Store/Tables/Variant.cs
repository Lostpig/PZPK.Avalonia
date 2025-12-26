using SQLite;

namespace PZPK.Store.Tables;

[Table(VariantTable.TableName)]
public class Variant
{
    [PrimaryKey]
    [Column(VariantTable.ColKey)]
    public string Key { get; set; } = "";

    [Column(VariantTable.ColValue)]
    public string Value { get; set; } = "";
}

internal static class VariantTable
{
    public const string TableName = "t_variant";
    public const string ColKey = "key";
    public const string ColValue = "value";
}

internal static class VariantKeys
{
    public const string BasePath = "basepath";
}