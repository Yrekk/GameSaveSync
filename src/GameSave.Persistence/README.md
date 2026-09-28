# GameSave.Persistence

Concrete central metadata persistence for GameSaveSync.

## Boundary

This project owns EF Core, the SQLite provider, `GameSaveDbContext`, persistence entities/mappings, EF migrations, concrete metadata repositories and SQLite-safe metadata snapshot implementation.

It implements ports defined by `GameSave.Application` when real application use cases require them.

It does not own HTTP endpoints, UI behavior, save-payload file storage or domain rules.

## H2.1B database bootstrap

The metadata database location is resolved explicitly from configuration.

Operational connections use SQLite `ReadWrite` mode so a missing database cannot be silently created by normal runtime inspection/use.

A separate explicitly named maintenance connection uses `ReadWriteCreate`. That path exists for future human-authorized initialization/migration operations only.

Server startup configures the DbContext but does not open, create or migrate the database.

## Migration policy

EF migration source code is authored during development, reviewed and versioned in Git.

Migrations are never applied automatically at startup.

The initial baseline migration intentionally contains no business tables. H2.2 will add schema only when real persistence use cases arrive.
