# GameSave.Application

Application-level use cases and the ports required by those use cases live here.

## Boundary

Application may depend on `GameSave.Core`.

It must not depend on EF Core/SQLite, ASP.NET Core, desktop/Web UI technology or NAS/SMB-specific implementations.

Repository and capability interfaces belong here only when a real use case requires them. H2.1B still avoids speculative interfaces.

Reusable administrative operations such as future database initialization, migration, snapshot and restore coordination belong here rather than in `Program.cs`, HTTP endpoints, Admin UI or IA/tool adapters.

This project is designed to remain reusable from Server adapters, HTTP endpoints, desktop/local UI, maintenance tooling and the future Web Admin/IA layer.
