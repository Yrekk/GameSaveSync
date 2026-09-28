using GameSave.Persistence.Database;
using Microsoft.Data.Sqlite;

namespace GameSave.Persistence.Tests;

public sealed class MetadataDatabaseConnectionStringsTests
{
    [Fact]
    public void OperationalConnection_UsesExistingDatabaseOnlyMode()
    {
        var settings = CreateSettings();

        var builder = new SqliteConnectionStringBuilder(
            MetadataDatabaseConnectionStrings.ForOperationalUse(settings));

        Assert.Equal(SqliteOpenMode.ReadWrite, builder.Mode);
        Assert.True(builder.ForeignKeys);
    }

    [Fact]
    public void ExplicitInitializationConnection_AllowsDatabaseCreation()
    {
        var settings = CreateSettings();

        var builder = new SqliteConnectionStringBuilder(
            MetadataDatabaseConnectionStrings.ForExplicitInitialization(settings));

        Assert.Equal(SqliteOpenMode.ReadWriteCreate, builder.Mode);
        Assert.True(builder.ForeignKeys);
    }

    private static MetadataDatabaseSettings CreateSettings()
    {
        return MetadataDatabaseSettings.FromConfiguredPath(
            Path.Combine("data", "metadata.db"),
            Path.GetTempPath());
    }
}
