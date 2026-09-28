using System.Reflection;

namespace GameSave.Storage.Tests;

public sealed class DependencyBoundaryTests
{
    [Fact]
    public void StorageAssembly_DoesNotReferenceMetadataPersistenceFrameworks()
    {
        var references = Assembly
            .Load("GameSave.Storage")
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase));
    }
}
