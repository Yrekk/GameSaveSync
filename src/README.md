# Source projects

The source tree is organized by architectural responsibility.

H0 created the initial project boundaries. H1 introduced the pure domain. H2.1A split application orchestration, metadata persistence and save-payload storage into explicit modules. H2.1B established the first real SQLite/bootstrap/migration foundation, H2.1C added validated read-only metadata database inspection, and H2.1D added validated fail-closed metadata readiness/safe-capability policy without introducing mutation workflows.

## Current boundaries

- **GameSave.Core** — pure domain layer. Owns synchronization rules, profile concepts and stable domain identities. It remains independent from ASP.NET Core, EF Core, Windows APIs, filesystem/NAS implementations and UI technology.
- **GameSave.Application** — application use cases and ports. It may depend on Core, but remains framework- and infrastructure-independent.
- **GameSave.Contracts** — transport/shared boundary contracts only when a real cross-process or API contract requires them. It is not a dumping ground for shared models.
- **GameSave.Persistence** — concrete central metadata persistence. Owns EF Core, SQLite, internal `GameSaveDbContext`, mappings/migrations/repositories and SQLite-safe metadata snapshot infrastructure.
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

## Current H2 state

Validated H2.1A:

- `GameSave.Application`, `GameSave.Persistence` and `GameSave.Storage`;
- matching focused test projects;
- EF Core + SQLite inside Persistence only;
- thin Server composition root;
- GUID-backed `MachineId` in Core.

Validated H2.1B:

- validated metadata DB path;
- existing-DB-only operational SQLite access;
- explicit separate database initialization semantics;
- empty versioned EF baseline migration;
- internal DbContext/EF infrastructure;
- no automatic startup migration;
- initialization, migration and restore kept as separate administrative decisions;
- reusable future administrative orchestration assigned to Application.

Validated H2.1C:

- provider-neutral inspection contract in Application;
- SQLite/EF inspection implementation in Persistence;
- immutable observed facts separated from semantic classification;
- Nexus-compatible structured findings and lifecycle state vocabulary;
- fact-compatible candidate states plus non-authoritative suggestion;
- deterministic states remain deterministic;
- inspection remains read-only with no lifecycle mutation.

Validated H2.1D:

- provider-neutral metadata operational modes in Application;
- abstract recovery availability separated from snapshot infrastructure;
- metadata authority separated from future whole-system synchronization readiness;
- readiness-safe capabilities separated from authorization and implementation availability;
- unresolved classification remains fail-closed;
- `OutOfService` preserves diagnostics/retry without pretending recovery is available;
- readiness evaluation remains read-only.

Still deferred:

- profile/business persistence schema;
- save-payload backend;
- business HTTP endpoint;
- Agent synchronization behavior.

Project-local READMEs describe each boundary in more detail and should be updated whenever a project's responsibility changes.
