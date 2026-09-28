# GameSave.Application

Application-level use cases and the ports required by those use cases live here.

## Boundary

Application may depend on `GameSave.Core`.

It must not depend on EF Core/SQLite, ASP.NET Core, desktop/Web UI technology or NAS/SMB-specific implementations.

Repository and capability interfaces belong here only when a real use case requires them. H2.1B still avoids speculative interfaces.

Reusable administrative operations such as future database initialization, migration, snapshot and restore coordination belong here rather than in `Program.cs`, HTTP endpoints, Admin UI or IA/tool adapters.

This project is designed to remain reusable from Server adapters, HTTP endpoints, desktop/local UI, maintenance tooling and the future Web Admin/IA layer.

## H2.1C metadata database inspection

Application owns the read-only inspection contract: state vocabulary, observed facts, candidate classifications, suggestion/reasons, the provider port and the reusable inspection use case.

The result deliberately has no authoritative selected `State` property. Admin/CLI/IA adapters may later present the candidates, but classification authority and lifecycle mutation remain separate concerns.

The inspection use case must not initialize, migrate, restore, bind or repair the database as a side effect.

Shared lifecycle/readiness and inspection/classification rules live in NexusPrincipia. GameSaveSync keeps only project-specific state semantics and safety policy.
