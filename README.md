# GameSaveSync

Generic and safe game save synchronization between multiple PCs, using Windows agents, a central .NET server and NAS-backed versioned storage.

> **Status:** pre-V1 — H1 accepted and merged to `develop`; H2.1A central-server foundation fully validated on `feature/h2-central-server`. Next step: define H2.1B.

## Purpose

GameSaveSync is intended to synchronize local game data between several PCs through a central server and versioned storage.

Project Zomboid will be the first real profile used to validate the system, but the repository is deliberately structured around generic synchronization components rather than one game.

## Current architecture

The architecture now separates domain rules, application use cases, metadata persistence, save-payload storage, transport hosting and Windows-side concerns.

```text
Windows PC(s)
  GameSave.Agent.UI
        ↓
  GameSave.Agent
        │
        │ HTTP/API
        ▼
  GameSave.Server
        │
        ├── GameSave.Application
        │      ↓
        │   GameSave.Core
        │
        ├── GameSave.Persistence
        │      └── EF Core / SQLite metadata
        │
        └── GameSave.Storage
               └── save payloads / file artifacts

Future real storage target: Custodia
```

Important boundaries:

- **GameSave.Core** — pure domain rules and value objects; no infrastructure dependency.
- **GameSave.Application** — application use cases and ports; no EF Core, ASP.NET, desktop UI or SMB/NAS implementation.
- **GameSave.Persistence** — concrete metadata persistence with EF Core + SQLite.
- **GameSave.Storage** — concrete save-payload/file storage implementations, deliberately separate from metadata persistence.
- **GameSave.Contracts** — transport/shared boundary contracts only when a real boundary needs them.
- **GameSave.Server** — ASP.NET Core host and composition root; not a second domain or persistence layer.
- **GameSave.Agent** — future Windows-side engine.
- **GameSave.Agent.UI** — future replaceable local UI adapter.

See [src/README.md](src/README.md) and the project-local READMEs for detailed boundaries.

## Validated H1 domain

H1 contains the pure deterministic synchronization and game-profile domain:

- explicit local/central synchronization state;
- deterministic synchronization assessments and cumulative findings;
- strictly-positive published synchronization versions with explicit no-version state;
- complete always-valid game profiles;
- logical data roots and per-machine path overrides;
- stable GUID-backed `MachineId`;
- optional managed-recovery configuration.

H1 was explicitly accepted and merged into `develop`.

## Current H2 state

H2 turns the existing server host into the first real central-authority boundary without pulling Agent, Windows lifecycle, real Custodia storage or save-transfer behavior forward.

H2.1A is fully validated and has established:

- `GameSave.Application`;
- `GameSave.Persistence`;
- `GameSave.Storage`;
- matching focused test projects;
- EF Core + SQLite isolated inside Persistence;
- an intentionally empty `GameSaveDbContext` with no speculative business tables;
- Server as a thin composition root;
- the hardened GUID-backed `MachineId` identity model.

Still intentionally deferred:

- business/profile persistence schema and repositories;
- runtime migration/snapshot/recovery coordinator;
- local save-artifact backend;
- first system-status endpoint;
- full machine registry;
- Agent behavior and real save transfers.

The next implementation slice is H2.1B, which must be defined before coding.

## Repository structure

```text
src/
  GameSave.Core/
  GameSave.Application/
  GameSave.Contracts/
  GameSave.Persistence/
  GameSave.Storage/
  GameSave.Server/
  GameSave.Agent/
  GameSave.Agent.UI/

tests/
  GameSave.Core.Tests/
  GameSave.Application.Tests/
  GameSave.Persistence.Tests/
  GameSave.Storage.Tests/
  GameSave.Server.Tests/
  GameSave.IntegrationTests/

docs/
  architecture/
  continuity/
  decisions/
  development/
  tranches/
```

See [docs/architecture/README.md](docs/architecture/README.md) for the project boundaries and [docs/tranches/H2-central-server.md](docs/tranches/H2-central-server.md) for the active tranche.

## Documentation and continuity

Documentation is part of the implementation.

A new development session starts with:

1. this README;
2. [docs/continuity/CURRENT_HANDOFF.md](docs/continuity/CURRENT_HANDOFF.md);
3. the active tranche document;
4. verification of the actual remote branch and HEAD;
5. inspection of the code relevant to the next action.

Shared development conventions are maintained in [NexusPrincipia](https://github.com/Yrekk/NexusPrincipia). GameSaveSync keeps only project-specific workflow rules locally.

## Development workflow

See [docs/development/WORKFLOW.md](docs/development/WORKFLOW.md).

## Build

Prerequisite: .NET 10 SDK.

```bash
dotnet restore GameSaveSync.sln
dotnet build GameSaveSync.sln --configuration Release --no-restore
dotnet test GameSaveSync.sln --configuration Release --no-build
```

## Roadmap

See [docs/roadmap.md](docs/roadmap.md).
