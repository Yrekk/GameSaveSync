# Current handoff

**Date:** 27 September 2026  
**Repository:** `Yrekk/GameSaveSync`  
**Working branch:** `feature/h2-central-server`  
**Integration branch:** `develop`  
**Deployment branch:** `deploy/succumbrae`  
**Stable branch:** `main`  
**Current tranche:** H2.1A — Modular persistence foundation (validated)

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

## VALIDATED H2 WORK

H2.1A is validated.

Remote CI is green: Release build 0 warnings/0 errors; 62 executed tests passed (59 Core + 1 Application + 1 Persistence + 1 Storage), with only the two expected no-test notices for Server.Tests and IntegrationTests.

Damien also completed local validation successfully: 62 tests passed, 0 failed, with the same two expected warnings.

Validated H2.1A scope:

- `GameSave.Application`, `GameSave.Persistence`, `GameSave.Storage`;
- focused Application/Persistence/Storage test projects;
- EF Core + SQLite isolated to Persistence;
- empty `GameSaveDbContext` foundation;
- `MachineId` value object in Core;
- `MachinePathOverride` now requires `MachineId`;
- module/dependency boundary tests and documentation.

No profile persistence schema, migrations, runtime recovery coordinator, storage backend or HTTP endpoint is implemented yet.

## IMPLEMENTED BUT NOT YET VALIDATED

None.

## DECIDED BUT NOT YET IMPLEMENTED

### H2 central server

H2 will turn the existing ASP.NET Core host into the first real central application/persistence boundary.

### Modular application/persistence boundaries

- H2 introduces separate `GameSave.Application` and `GameSave.Persistence` projects;
- Application owns use cases and repository/capability ports;
- Persistence owns EF Core/SQLite implementations, entities, mappings and migrations;
- Server remains a thin ASP.NET Core host/composition root;
- Contracts remains transport-only;
- modules are designed to be extractable into reusable libraries/services later when a real second consumer exists;
- do not prematurely genericize or create microservices without an actual operational/reuse need.

See `docs/decisions/ADR-0005-application-persistence-modules.md`.

### Save payload storage boundary

- H2 introduces a separate `GameSave.Storage` project;
- Application owns GameSaveSync-specific storage ports;
- Storage provides the concrete local-filesystem backend in H2;
- Persistence remains metadata/EF/SQLite only;
- the storage port is not a generic filesystem API;
- H2 proves basic artifact store/read/existence behavior only;
- H5/H8 retain transactional transfers/version publication and real Custodia integration;
- Storage remains extractable/reusable later without premature microservice deployment.

See `docs/decisions/ADR-0006-separate-storage-module.md`.

### First transport use case

- first Application/network use case is `GetSystemStatus`;
- initial endpoint is read-only `GET /api/system/status`;
- it reports operational mode/readiness, including database/migration and storage health relevant to H2;
- synchronization authority availability is explicit;
- process liveness is a separate concept and must not be confused with readiness;
- desktop/local UI and future Web Admin will reuse the same Application use case;
- Contracts now has a real transport reason, but must not expose EF entities or Core models directly.

See `docs/decisions/ADR-0007-first-system-status-api.md`.

### Machine identity

- H2 introduces a stable opaque `MachineId` value object;
- MachineId identifies a logical GameSaveSync machine, not hostname, username or a specific Agent installation;
- a new PC always receives a new MachineId;
- a reformatted/reinstalled machine may retain its existing MachineId only through a future explicit/authorized rebind flow;
- hostname, username, workgroup and path data remain mutable metadata;
- username may later be persisted because it can affect per-machine paths, but it never defines identity;
- H2 does not create a full machine registry/table yet; path overrides may persist opaque MachineId values until H3 adds the real registry consumer.

See `docs/decisions/ADR-0008-stable-machine-identity.md`.

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
3. repository boundary placement — RESOLVED: Application ports + Persistence implementations in separate projects;
4. Server failure policy — RESOLVED: restricted recovery mode + explicit validated snapshot restore;
5. exact H2 fake/local storage scope — RESOLVED: separate Storage module + minimal local backend;
6. first real API use case — RESOLVED: read-only GetSystemStatus / `GET /api/system/status`;
7. minimum machine metadata — RESOLVED: stable opaque MachineId only; mutable host metadata deferred.

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

Walk through the new H2.1A projects/files in Visual Studio and explain the dependency direction and responsibilities before defining the next H2 slice.
