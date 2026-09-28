using GameSave.Application.MetadataDatabase;
using GameSave.Application.SystemStatus;
using GameSave.Server.SystemStatus;
using GameSave.Persistence.Database;
using GameSave.Storage;
using GameSave.Storage.Local;

var builder = WebApplication.CreateBuilder(args);

var configuredMetadataPath =
    builder.Configuration["GameSave:MetadataDatabase:Path"];
var configuredControlStorePath =
    builder.Configuration["GameSave:ControlStore:Path"];
var configuredStoragePath =
    builder.Configuration["GameSave:Storage:RootPath"];
var configuredSnapshotPath =
    builder.Configuration["GameSave:MetadataSnapshots:Path"];

var metadataDatabaseSettings =
    MetadataDatabaseSettings.FromConfiguredPath(
        configuredMetadataPath,
        builder.Environment.ContentRootPath);
var controlStoreSettings =
    MetadataDatabaseControlStoreSettings.FromConfiguredPath(
        configuredControlStorePath,
        builder.Environment.ContentRootPath);
var storageSettings =
    LocalSaveArtifactStorageSettings.FromConfiguredPath(
        configuredStoragePath,
        builder.Environment.ContentRootPath);
var snapshotSettings =
    MetadataDatabaseSnapshotSettings.FromConfiguredPath(
        configuredSnapshotPath,
        builder.Environment.ContentRootPath);

builder.Services.AddGameSavePersistence(
    metadataDatabaseSettings,
    controlStoreSettings,
    snapshotSettings);
builder.Services.AddGameSaveStorage(storageSettings);

builder.Services.AddScoped<InspectMetadataDatabase>();
builder.Services.AddScoped<ResolveMetadataDatabaseClassification>();
builder.Services.AddScoped<InspectAndResolveMetadataDatabase>();
builder.Services.AddScoped<InitializeMetadataDatabase>();
builder.Services.AddScoped<ApplyPendingMetadataDatabaseMigrations>();
builder.Services.AddScoped<CreateRollingMetadataDatabaseSnapshot>();
builder.Services.AddScoped<RestoreMetadataDatabaseSnapshot>();
builder.Services.AddScoped<GetSystemStatus>();

var app = builder.Build();

// Startup composes persistence but never opens, creates or migrates the metadata
// database implicitly. Migration execution is an explicit administrative operation.
app.MapGet(
    "/api/system/status",
    async (GetSystemStatus getStatus, CancellationToken cancellationToken) =>
    {
        var status = await getStatus.ExecuteAsync(cancellationToken);
        return Results.Ok(SystemStatusContractMapper.ToResponse(status));
    });

app.Run();

public partial class Program;
