# GameSaveSync

Generic and safe game save synchronization between multiple PCs, using Windows agents, a central .NET server and NAS-backed versioned storage.

> **Status:** pre-V1 — H1 domain consolidation in progress.

## Purpose

GameSaveSync is intended to synchronize local game data between several PCs through a central server and versioned storage.

Project Zomboid will be the first real profile used to validate the system, but the repository is deliberately structured around generic synchronization components rather than one game.

## H0 scope

H0 builds the **skeleton only**:

- solution and project boundaries;
- dependency direction;
- build conventions;
- ASP.NET Core server-host boundary;
- shared development launch configuration;
- test projects;
- CI;
- documentation and session-continuity workflow.

H0 contains **no synchronization business rule**. Domain concepts and synchronization decisions belong to H1 and later tranches.

H0 is validated both in CI and locally.

## Current H1 domain

H1 now contains the pure deterministic synchronization and game-profile domain:

- explicit local/central synchronization state;
- deterministic synchronization assessments and cumulative findings;
- strictly-positive published synchronization versions with explicit no-version state;
- complete always-valid game profiles;
- logical data roots and per-machine path overrides;
- optional managed-recovery configuration;
- SQLite accepted as the later Server metadata persistence direction while Core remains persistence-agnostic.

H1.4 is the consolidation/audit step before H2 introduces central-server infrastructure.

## Repository structure

```text
src/
  GameSave.Core/
  GameSave.Contracts/
  GameSave.Server/
  GameSave.Agent/
  GameSave.Agent.UI/

tests/
  GameSave.Core.Tests/
  GameSave.Server.Tests/
  GameSave.IntegrationTests/

docs/
  architecture/
  continuity/
  decisions/
  development/
  tranches/
```

See [docs/architecture/README.md](docs/architecture/README.md) for the project boundaries.

## Documentation and continuity

Documentation is part of the implementation.

A new development session starts with:

1. this README;
2. [docs/continuity/CURRENT_HANDOFF.md](docs/continuity/CURRENT_HANDOFF.md);
3. the active tranche document;
4. verification of the actual remote branch and HEAD;
5. inspection of the code relevant to the next action.

Meaningful architectural folders have concise READMEs explaining their role and boundaries. Trivial code and obvious DTO/model choices are not over-documented.

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
