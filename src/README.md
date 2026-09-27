# Source projects

The source tree is organized by architectural responsibility.

H0 created the initial project boundaries. H1 introduced the pure domain. H2.1A then split application orchestration, metadata persistence and save-payload storage into explicit modules before any business persistence schema or transport API is added.

## Current boundaries

- **GameSave.Core** — pure domain layer. Owns synchronization rules, profile concepts and stable domain identities. It remains independent from ASP.NET Core, EF Core, Windows APIs, filesystem/NAS implementations and UI technology.
- **GameSave.Application** — application use cases and ports. It may depend on Core, but remains framework- and infrastructure-independent.
- **GameSave.Contracts** — transport/shared boundary contracts only when a real cross-process or API contract requires them. It is not a dumping ground for shared models.
- **GameSave.Persistence** — concrete central metadata persistence. Owns EF Core, SQLite, `GameSaveDbContext`, future mappings/migrations/repositories and SQLite-safe metadata snapshot infrastructure.
- **GameSave.Storage** — concrete save-payload and file-artifact storage implementations. It is deliberately separate from metadata persistence.
- **GameSave.Server** — ASP.NET Core host and composition root running on Succumbrae. It wires Application to concrete Persistence/Storage implementations and exposes transport boundaries, but must not duplicate domain rules.
- **GameSave.Agent** — future Windows-side synchronization engine. Process monitoring, filesystem watching, lifecycle handling and transfer behavior belong here when their tranches arrive.
- **GameSave.Agent.UI** — future replaceable local Windows UI adapter. It must not own synchronization business rules.

## Dependency direction

The architectural intent is inward-facing:

```text
UI / transport / infrastructure
            ↓
       Application
            ↓
          Core
```

Concrete infrastructure implements capabilities required by Application:

```text
GameSave.Persistence ─┐
GameSave.Storage ─────┼─→ Application → Core
GameSave.Server ──────┘
```

The exact project references are kept as narrow as the current implementation requires.

## Current H2.1A state

H2.1A has already materialized:

- `GameSave.Application`;
- `GameSave.Persistence`;
- `GameSave.Storage`;
- matching focused test projects;
- EF Core + SQLite inside Persistence only;
- an intentionally empty `GameSaveDbContext`;
- a thin Server composition root;
- GUID-backed `MachineId` in Core.

It still deliberately contains no profile/business persistence schema, no save-payload backend, no business HTTP endpoint and no Agent synchronization behavior.

Project-local READMEs describe each boundary in more detail and should be updated when a project's responsibility changes.
