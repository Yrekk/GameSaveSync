using System.Net;
using System.Net.Http.Json;
using GameSave.Contracts.SystemStatus;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace GameSave.IntegrationTests;

public sealed class SystemStatusEndpointTests
{
    [Fact]
    public async Task GetSystemStatus_WhenMetadataIsMissing_ReturnsMaintenance()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "GameSaveSync.IntegrationTests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(root);

        try
        {
            await using var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(
                    builder =>
                    {
                        builder.UseEnvironment("Development");
                        builder.ConfigureAppConfiguration(
                            (_, configuration) =>
                            {
                                configuration.AddInMemoryCollection(
                                    new Dictionary<string, string?>
                                    {
                                        ["GameSave:MetadataDatabase:Path"] =
                                            Path.Combine(root, "metadata.db"),
                                        ["GameSave:ControlStore:Path"] =
                                            Path.Combine(root, "control.xml"),
                                        ["GameSave:Storage:RootPath"] =
                                            Path.Combine(root, "storage"),
                                        ["GameSave:MetadataSnapshots:Path"] =
                                            Path.Combine(root, "snapshots"),
                                    });
                            });
                    });

            using var client = factory.CreateClient();

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
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
