using System.Reflection;

namespace GameSave.Application.Tests;

public sealed class DependencyBoundaryTests
{
    [Fact]
    public void ApplicationAssembly_DoesNotReferenceInfrastructureFrameworks()
    {
        var references = Assembly
            .Load("GameSave.Application")
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal));
        Assert.DoesNotContain(references, name => name.Contains("Sqlite", StringComparison.OrdinalIgnoreCase));
    }
}
