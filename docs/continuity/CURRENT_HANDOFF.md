# Current handoff

**Date:** 27 September 2026  
**Repository:** `Yrekk/GameSaveSync`  
**Working branch:** `feature/h2-central-server`  
**Integration branch:** `develop`  
**Deployment branch:** `deploy/succumbrae`  
**Stable branch:** `main`  
**Current tranche:** H2 — Minimal central server (planning)

Always verify the actual remote branch and HEAD before modifying the repository.

## VALIDATED

- H0 bootstrap is validated and stable.
- H1 Generic deterministic domain is fully validated, explicitly accepted by Damien and merged into `develop`.
- H1 final local validation: Release build green, 51/51 tests passing, diff-check clean, working tree clean.
- H1 includes:
  - explicit synchronization/no-version semantics;
  - deterministic `SyncAssessment` + cumulative findings;
  - integrity/conflict vocabulary;
  - complete always-valid generic `GameProfile`;
  - logical data roots and per-machine overrides;
  - optional managed-recovery configuration;
  - ADR-0001 accepting central SQLite metadata persistence.
- Branch promotion remains `feature/* → develop → deploy/succumbrae → main`.
- H1 stopped at `develop`; no deployment/main promotion was authorized.

## IMPLEMENTED BUT NOT YET VALIDATED

None for H2 application code.

The H2 branch currently contains planning/documentation only.

## DECIDED BUT NOT YET IMPLEMENTED

### H2 central server

H2 will turn the existing ASP.NET Core host into the first real central application/persistence boundary.

### Persistence access

- EF Core is the default persistence layer;
- SQLite is the initial provider;
- Core remains independent from EF Core and SQLite;
- persistence entities/mappings live in Server infrastructure;
- targeted raw SQL is allowed only when explicitly justified and isolated;
- no lazy loading;
- provider/database changes remain explicit migration projects, not assumed automatic.

See `docs/decisions/ADR-0002-ef-core-sqlite-persistence.md`.

### SQLite

- one central SQLite database for GameSaveSync metadata/configuration;
- active DB local to Succumbrae;
- not hosted live on Custodia/SMB;
- safe backup to Custodia later;
- save payloads remain files;
- Core remains SQLite-independent.

### Administrative operation reuse

- database migration/snapshot/recovery logic is implemented once as Server application use cases;
- desktop/local UI, future Web Admin, startup and possible maintenance CLI are only entry-point adapters;
- EF migration classes are authored during development and versioned in Git;
- deployed interfaces may execute already-known migrations but do not dynamically author migration source code;
- startup automatic migration must call the same coordinator as manual/Admin execution;
- authoritative DB operations execute on Succumbrae, even when requested remotely;
- authorization for destructive/admin operations will be defined at the transport/Admin boundary.

See `docs/decisions/ADR-0004-reusable-administrative-use-cases.md`.

### Metadata database recovery

- keep SQLite-safe known-good metadata snapshots;
- initial retention direction is at least the two most recent rolling snapshots;
- create a pre-migration snapshot before data-changing schema migration;
- database failure enters restricted recovery/minimal mode;
- normal synchronization authority is disabled in that mode;
- snapshot restoration requires explicit administrative selection/confirmation;
- restored metadata must be validated before normal mode resumes;
- later, when real save-version storage exists, metadata recovery must reconcile against that storage to prevent authority rewind.

See `docs/decisions/ADR-0003-metadata-snapshot-recovery.md`.

### Managed save recovery

Managed recovery checkpoints for game saves remain parallel to normal synchronization and are separate from metadata database snapshots. They are not implemented in H2 unless a later explicitly accepted H2 design requires only metadata needed by future recovery work.

### Operational diagnostics

Structured diagnostics remain a cross-cutting requirement. H2 should avoid designs that make later structured Server diagnostics difficult, but the Admin live stream is not an H2 deliverable.

## OPEN H2 DESIGN QUESTIONS

Read `docs/tranches/H2-central-server.md` before implementation.

The first discussion must resolve:

1. SQLite access style — RESOLVED: EF Core + SQLite provider;
2. migration/schema versioning — RESOLVED: versioned EF migrations + reusable execution coordinator;
3. repository boundary placement;
4. Server failure policy — RESOLVED: restricted recovery mode + explicit validated snapshot restore;
5. exact H2 fake/local storage scope;
6. first real API use case;
7. minimum machine metadata required now.

Do not silently answer these in code.

## CURRENT SERVER STATE

`GameSave.Server` is still the H0 ASP.NET Core host:

- no business endpoint;
- no controller;
- no transport DTO;
- no persistence implementation;
- no storage implementation.

`GameSave.Contracts` is still intentionally empty of DTOs until a real boundary requires one.

## EXPECTED TEST WARNINGS

Until H2 adds real Server/Integration behavior, these warnings may still appear and are known:

- `GameSave.Server.Tests`: no tests available;
- `GameSave.IntegrationTests`: no tests available.

As soon as H2 adds real Server behavior, the first warning should naturally disappear because real Server tests should exist. Do not add fake tests.

## READ FIRST NEXT SESSION

1. root `README.md`;
2. this file;
3. `docs/tranches/H2-central-server.md`;
4. `docs/decisions/ADR-0001-server-metadata-sqlite.md`;
5. `src/GameSave.Server/README.md`;
6. `src/GameSave.Contracts/README.md`;
7. `src/GameSave.Server/Program.cs`;
8. actual remote branch and HEAD.

## NEXT EXACT ACTION

Explain the current Server/Contracts skeleton and work through the H2 design questions with Damien before implementing H2.1.
