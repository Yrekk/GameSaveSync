using System.Net;
using System.Net.Http.Json;
using GameSave.Contracts.SystemStatus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace GameSave.IntegrationTests;

public sealed class SystemStatusEndpointTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SystemStatusEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(
            builder => builder.UseEnvironment("Development"));
    }

    [Fact]
    public async Task GetSystemStatus_WhenMetadataIsMissing_ReturnsMaintenance()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/system/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var status = await response.Content
            .ReadFromJsonAsync<SystemStatusResponse>();

        Assert.NotNull(status);
        Assert.Equal("Maintenance", status.Mode);
        Assert.False(status.SynchronizationAvailable);
        Assert.Equal("Missing", status.Metadata.State);
        Assert.Equal("Maintenance", status.Metadata.Mode);
        Assert.Equal("Ready", status.Storage.Status);
    }
}
