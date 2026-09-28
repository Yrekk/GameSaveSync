using Microsoft.Data.Sqlite;

namespace GameSave.Persistence.Database;

/// <summary>
/// Builds SQLite connection strings with explicit creation semantics.
/// </summary>
internal static class MetadataDatabaseConnectionStrings
{
    public static string ForOperationalUse(MetadataDatabaseSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return Build(settings.DatabasePath, SqliteOpenMode.ReadWrite);
    }

    public static string ForExplicitInitialization(MetadataDatabaseSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return Build(settings.DatabasePath, SqliteOpenMode.ReadWriteCreate);
    }

    private static string Build(string databasePath, SqliteOpenMode mode)
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = mode,
            ForeignKeys = true,
        }.ToString();
    }
}
