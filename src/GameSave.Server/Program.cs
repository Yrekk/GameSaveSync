using GameSave.Persistence.Database;

var builder = WebApplication.CreateBuilder(args);

var configuredMetadataPath =
    builder.Configuration["GameSave:MetadataDatabase:Path"];
var configuredControlStorePath =
    builder.Configuration["GameSave:ControlStore:Path"];

var metadataDatabaseSettings =
    MetadataDatabaseSettings.FromConfiguredPath(
        configuredMetadataPath,
        builder.Environment.ContentRootPath);
var controlStoreSettings =
    MetadataDatabaseControlStoreSettings.FromConfiguredPath(
        configuredControlStorePath,
        builder.Environment.ContentRootPath);

builder.Services.AddGameSavePersistence(
    metadataDatabaseSettings,
    controlStoreSettings);

var app = builder.Build();

// Startup composes persistence but never opens, creates or migrates the metadata
// database implicitly. Migration execution is an explicit administrative operation.
app.Run();
