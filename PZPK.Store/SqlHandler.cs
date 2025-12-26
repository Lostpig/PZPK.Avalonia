using SQLite;

namespace PZPK.Store;

internal sealed class SqlHandler : IDisposable
{
    public static SqlHandler Open(string dbPath, string password)
    {
        if (!File.Exists(dbPath))
        {
            throw new FileNotFoundException("Sqlite db file not found", dbPath);
        }

        return new SqlHandler(dbPath, password);
    }
    public static SqlHandler Create(string dbPath, string password)
    {
        if (!File.Exists(dbPath))
        {
            throw new ArgumentException("db file already exists", nameof(dbPath));
        }

        var db = new SqlHandler(dbPath, password);
        db.Initialize();

        return db;
    }

    public SQLiteConnection DB { get; init; }

    private SqlHandler(string dbPath, string password)
    {
        var connStr = new SQLiteConnectionString(dbPath, true, key: password);
        DB = new SQLiteConnection(connStr);
    }

    private void Initialize()
    {
        DB.BeginTransaction();

        DB.CreateTable<Tables.Package>();
        DB.CreateTable<Tables.Tag>();
        DB.CreateTable<Tables.Variant>();

        DB.Commit();
    }

    public void Dispose()
    {
        DB.Dispose();
    }
}
