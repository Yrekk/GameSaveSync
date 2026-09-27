# GameSave.Persistence

Concrete central metadata persistence for GameSaveSync.

## Boundary

This project owns EF Core, the SQLite provider, `GameSaveDbContext`, future persistence entities/mappings, EF migrations, concrete metadata repositories and SQLite-safe metadata snapshot implementation.

It implements ports defined by `GameSave.Application`.

It does not own HTTP endpoints, UI behavior, save-payload file storage or domain rules.

H2.1A establishes the EF/SQLite foundation without inventing business tables before their real persistence slice.
