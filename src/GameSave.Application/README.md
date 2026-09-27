# GameSave.Application

Application-level use cases and the ports required by those use cases live here.

## Boundary

Application may depend on `GameSave.Core`.

It must not depend on EF Core/SQLite, ASP.NET Core, desktop/Web UI technology or NAS/SMB-specific implementations.

Repository and capability interfaces belong here only when a real use case requires them. H2.1A intentionally does not pre-create speculative interfaces.

This project is designed to remain reusable from Server startup, HTTP endpoints, desktop/local UI and the future Web Admin.
