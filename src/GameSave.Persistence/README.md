# GameSave.Persistence

Concrete central metadata persistence for GameSaveSync.

## Boundary

This project owns EF Core, the SQLite provider, `GameSaveDbContext`, persistence entities/mappings, EF migrations, concrete metadata repositories and SQLite-safe metadata snapshot implementation.

It implements ports defined by `GameSave.Application` when real application use cases require them.

It does not own HTTP endpoints, UI behavior, save-payload file storage or domain rules.

EF infrastructure such as `GameSaveDbContext`, its design-time factory and low-level connection builders remains internal so Server/adapters cannot bypass Application boundaries accidentally.

## H2.1B database bootstrap

The metadata database location is resolved explicitly from configuration.

Operational connections use SQLite `ReadWrite` mode so a missing database cannot be silently created by normal runtime inspection/use.

A separate explicitly named initialization connection uses `ReadWriteCreate`. It exists only for a future human-authorized initial database creation.

Migrating an existing database must use existing-database semantics and must never implicitly initialize a missing database.

Server startup configures Persistence but does not open, create or migrate the database.

## Migration policy

EF migration source code is authored during development, reviewed and versioned in Git.

Migrations are never applied automatically at startup.

The baseline migration intentionally contains no business tables. H2.2 will add schema only when real persistence use cases arrive.

## H2.1C metadata database inspection

Persistence implements the Application inspection port using SQLite and EF migration metadata.

It observes file/path presence, accessibility, SQLite integrity, migration history, current-binary migrations and non-system user tables. It returns facts plus Nexus-compatible structured findings, safe candidate classifications and a suggestion.

Inspection preserves H2.1B safety:

- no missing database creation;
- no initialization;
- no migration execution;
- no restore/repair side effect;
- no authoritative administrative classification;
- no automatic action selection.

The shared lifecycle/readiness and inspection/classification rules live in NexusPrincipia.
