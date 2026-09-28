# GameSave.Storage

Concrete save-payload/file storage implementations live here.

## Boundary

Storage implements GameSaveSync-specific storage ports defined by `GameSave.Application`.

It is deliberately separate from `GameSave.Persistence`:

- Persistence = metadata/configuration database;
- Storage = save payloads and file artifacts.

H2.1A establishes only the module boundary. The minimal local-filesystem backend is implemented in a later H2 slice, before Custodia integration.

Do not expose a generic arbitrary-filesystem API to Application.
