# H2 — Minimal central server

**Status:** H2.1B IMPLEMENTED — REMOTE CI GREEN — LOCAL VALIDATION PENDING  
**Branch:** `feature/h2-central-server`  
**Base:** `develop` after accepted H1 merge

## Objective

Turn the existing ASP.NET Core host into the first real central authority boundary without pulling Agent, Windows lifecycle, real Custodia storage or save-transfer behavior forward.

H2 begins from the accepted H1 domain and ADR-0001 SQLite direction.

## What H2 is allowed to introduce

- Server-side application/persistence boundaries;
- a separate `GameSave.Storage` module with a minimal local filesystem backend behind Application ports;
- SQLite metadata persistence on Succumbrae local storage;
- schema/bootstrap/migration strategy;
- persistence of complete valid domain data such as `GameProfile`;
- central metadata needed by later synchronization work;
- minimal API/contracts only when a real H2 use case requires them;
- Server unit/integration tests.

## What H2 must not implement yet

- Windows process detection;
- local filesystem watchers;
- game lifecycle handling;
- real Custodia/NAS save payload storage;
- reliable save transfer protocol;
- managed recovery checkpoint execution;
- Project Zomboid-specific behavior;
- Admin UI.

## Accepted constraints carried from H1

### Domain ownership

`GameSave.Core` remains pure and persistence-agnostic.

The Server consumes Core domain objects and must not duplicate synchronization/profile rules.

### SQLite direction

ADR-0001 is accepted:

- one central SQLite database for GameSaveSync configuration/metadata;
- active DB on Succumbrae local storage;
- never host the active DB directly on Custodia/SMB;
- Custodia later receives SQLite-safe backups;
- save payloads stay as files, not SQLite blobs.

### Profile invariant

A persisted `GameProfile` is already complete and structurally valid.

Incomplete UI drafts do not belong in the database.

Global `ProfileId` uniqueness is a persistence/database responsibility.

## Active implementation slice — H2.1A modular persistence foundation

H2.1A materializes the accepted module boundaries without pulling later behavior forward.

Implemented scope:

- add `GameSave.Application`, `GameSave.Persistence` and `GameSave.Storage` projects;
- add matching focused test projects;
- wire dependency direction so Application stays framework-independent and Server remains the composition root;
- add EF Core + SQLite to Persistence only;
- add an intentionally empty `GameSaveDbContext` with no speculative business tables;
- introduce `MachineId` in Core and replace raw machine-id strings in `MachinePathOverride`; post-review hardening changes its representation to a non-empty GUID;
- add architecture/domain tests and module documentation.

Explicitly deferred from H2.1A:

- profile persistence schema/repositories;
- EF migrations containing business tables;
- runtime migration/snapshot/recovery coordinator;
- local save-artifact backend;
- system-status endpoint;
- full Machine registry.

## Active implementation slice — H2.1B metadata database bootstrap

H2.1B builds the first real SQLite/migration plumbing without introducing business tables or automatic migration execution.

Scope:

- resolve and validate an explicit metadata database path;
- configure `GameSaveDbContext` through Persistence;
- keep normal operational SQLite connections in existing-database-only mode so a missing DB is never silently created;
- provide a separate, explicitly named maintenance connection mode that may create the initial database when a human-authorized migration operation eventually uses it;
- add a versioned empty EF migration baseline so migration history starts before business schema exists;
- prove pending/applied migration inspection against a temporary SQLite database;
- wire the Server composition root to the configured metadata database without opening or migrating it;
- add a development-only deterministic relative database path resolved from the Server content root.

Explicitly deferred from H2.1B:

- automatic or manual runtime migration command/use case;
- pre-migration snapshot creation;
- recovery/minimal-mode coordinator;
- profile/business tables and repositories;
- system-status endpoint;
- save-artifact storage backend.

### Manual migration invariant

GameSaveSync does not apply schema/data migrations automatically at startup.

Startup may later inspect and report migration state, but applying a migration requires an explicit administrative action.

The eventual manual action will be implemented only with its snapshot/validation/recovery safety flow.

## Candidate H2 slices

These are planning candidates, not implementation commitments. They must be confirmed during the H2 design discussion.

### H2.1 — persistence foundation

Potential scope:

- EF Core + SQLite persistence foundation (ADR-0002);
- choose schema migration/versioning strategy;
- define DB configuration/bootstrap;
- separate `GameSave.Application` and `GameSave.Persistence` projects with repository ports in Application and EF/SQLite implementations in Persistence (ADR-0005);
- restricted recovery/minimal mode with validated metadata snapshots (ADR-0003);

### H2.2 — profile persistence

Potential scope:

- persist/reload `GameProfile`;
- process names;
- logical data roots;
- per-machine path overrides;
- exclusions;
- version retention;
- recovery policy;
- external-cloud warning;
- enforce global profile-id uniqueness transactionally.

### H2.3 — minimal central metadata

Potential scope:

- persist only the minimum central metadata required by later Agent/synchronization work;
- avoid inventing fields whose first real consumer does not exist yet.

### H2.4 — minimal transport boundary

Potential scope:

- introduce Contracts/HTTP DTOs only for a concrete H2 operation;
- keep transport models separate from Core models;
- test serialization/validation at the application boundary.

The exact API surface is deliberately undecided.

## Design questions to answer before implementation

1. **SQLite access style — RESOLVED:** EF Core with the SQLite provider is the default persistence layer. Core remains persistence-independent; raw SQL remains allowed only as an isolated, justified persistence escape hatch. See ADR-0002.
2. **Migrations — RESOLVED:** EF migration code is authored during development, reviewed and versioned in Git. Runtime execution uses a reusable Server application use case shared by startup, desktop/API and future Web Admin entry points. Known migrations may be applied automatically or explicitly through that same mechanism. Pre-migration snapshot, application, validation and recovery-mode failure handling are one implementation, never UI-specific. See ADR-0004.
3. **Repository placement — RESOLVED:** introduce separate `GameSave.Application` and `GameSave.Persistence` projects. Repository/capability interfaces belong to Application; EF/SQLite implementations belong to Persistence; Server remains a thin host/composition root; Contracts stays transport-only. Modules should be extractable later without prematurely becoming microservices. See ADR-0005.
4. **Startup/failure policy — RESOLVED:** fail closed for synchronization authority, keep a restricted recovery/diagnostic mode, and use explicit validated snapshot restoration. No automatic snapshot promotion. See ADR-0003.
5. **H2 storage scope — RESOLVED:** introduce a separate `GameSave.Storage` project. Application owns GameSaveSync-specific storage ports; Storage provides a minimal controlled local-filesystem backend in H2. H5/H8 retain transactional transfer/version publication and real Custodia integration. See ADR-0006.
6. **First real API use case — RESOLVED:** `GetSystemStatus` exposed initially as read-only `GET /api/system/status`. It reports application mode/readiness across metadata persistence and storage, while process liveness remains a separate concept. The use case lives in Application and Contracts carries only the transport DTO. See ADR-0007.
7. **Machine metadata — RESOLVED:** H2 introduces only a stable opaque `MachineId` value object backed by a non-empty GUID and uses it for existing per-machine configuration. Hostname, username, workgroup and paths are mutable metadata and never identity. No full machine registry is created until the Agent provides the real consumer. Reinstall/rebind to an existing MachineId must later be explicit/authorized; a new machine always gets a new ID. See ADR-0008.

No code should answer these silently. The design discussion comes first.

## Validation expectations

For each accepted H2 slice:

- Release build green;
- relevant Server tests added for real behavior;
- Integration tests begin only when a real cross-project boundary exists;
- no fake tests added merely to silence empty-project warnings;
- `git diff --check` clean;
- documentation/handoff current;
- local validation by Damien before tranche acceptance.

## Next exact action

H2.1B remote CI is green on commit `9c795a4`:

- Release build: 0 warnings, 0 errors;
- Core: 57 tests passed;
- Application: 1 test passed;
- Persistence: 8 tests passed;
- Storage: 1 test passed;
- total: 67 passed, 0 failed;
- Server.Tests and IntegrationTests retain their expected no-test notices.

The first H2.1B CI run exposed a Windows-only test cleanup issue: Microsoft.Data.Sqlite pooling kept the temporary database file open after the functional migration assertions succeeded. The fixture now clears SQLite pools before deleting its temporary directory. Product pooling remains unchanged.

Next:

1. Damien performs local Release build/tests/diff/status validation.
2. Review the H2.1B code and architecture together.
3. Correct any issue found during review.
4. Damien explicitly accepts H2.1B.
5. Only then define H2.1C.
