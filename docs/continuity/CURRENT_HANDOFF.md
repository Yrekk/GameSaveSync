# Current handoff

**Date:** 28 September 2026  
**Repository:** `Yrekk/GameSaveSync`  
**Working branch:** `feature/h2-central-server`  
**Integration branch:** `develop`  
**Deployment branch:** `deploy/succumbrae`  
**Stable branch:** `main`  
**Current tranche:** H2.1C — Metadata database inspection (fully validated and explicitly accepted; next H2 slice not yet scoped)

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
- `MachineId` value object in Core (original H2.1A representation superseded by the approved GUID hardening below);
- `MachinePathOverride` now requires `MachineId`;
- module/dependency boundary tests and documentation.

No profile persistence schema, migrations, runtime recovery coordinator, storage backend or HTTP endpoint is implemented yet.

## VALIDATED H2.1B

### H2.1B — Metadata database bootstrap & manual migration foundation

Implemented on `feature/h2-central-server`.

Remote validation is green after the shared review corrections:

- Release build: 0 warnings, 0 errors;
- Core tests: 57 passed;
- Application tests: 1 passed;
- Persistence tests: 9 passed;
- Storage tests: 1 passed;
- total executed tests: 68 passed, 0 failed;
- Server.Tests and IntegrationTests still have the two expected no-test notices.

H2.1B adds:

- explicit metadata database path resolution relative to a known base path;
- Persistence DI registration for `GameSaveDbContext`;
- operational SQLite connection mode = `ReadWrite`, preventing silent creation of a missing database;
- explicit initialization connection mode = `ReadWriteCreate`, reserved only for future human-authorized first-time database creation;
- versioned empty EF baseline migration `20260928000000_InitialMetadataDatabase`;
- design-time DbContext factory for migration authoring;
- development-only metadata DB path configuration;
- migration tests proving pending/applied state and reopen behavior;
- test proving normal operational access does not create a missing DB;
- Server composition wiring without opening, creating or migrating the DB.

Migration execution is deliberately NOT implemented yet.

GameSaveSync now has an explicit invariant: startup may inspect migration state later, but it never applies schema/data migrations automatically.

Commits:

- `cfb41d2` — H2.1B implementation;
- `9c795a4` — Windows SQLite test-pool cleanup fix.

Damien's initial local H2.1B validation was green before the shared review.

The shared review is now completed and produced structural corrections:

- test SQLite pooling is disabled locally instead of clearing all process pools;
- maintenance/creation connection helper is internal to Persistence;
- `GameSaveDbContext` and its design-time factory are internal to Persistence;
- an architecture test protects that EF infrastructure is not public;
- the baseline migration id was anchored safely before future generated migration ids;
- local Server metadata data is ignored by Git;
- initialization is explicitly separated from migration and restore;
- ADR-0004 was aligned with ADR-0005: reusable administrative use cases live in `GameSave.Application`, not in Server;
- startup remains observation/composition only and never decides among init/migrate/restore/recovery actions.

Damien completed the post-review local validation successfully and explicitly accepted H2.1B on 28 September 2026.


## VALIDATED H2.1C

### H2.1C — Read-only metadata database inspection

H2.1C is fully validated and explicitly accepted by Damien on 28 September 2026.

Final code HEAD before documentation closure:

```text
d7898c5f2c9d8e96dd23940d3490dbdac65f083c
```

Final CI:

- run `36407964589` — SUCCESS;
- Release build: 0 warnings, 0 errors;
- Core: 57 tests passed;
- Application: 7 tests passed;
- Persistence: 24 tests passed;
- Storage: 1 test passed;
- total executed: 89 passed, 0 failed;
- Server.Tests and IntegrationTests retain the two expected no-test notices.

Validated behavior:

- inspection is strictly read-only and never creates a missing DB;
- Application owns provider-neutral states, facts, findings, candidates, suggestion and the reusable inspection use case;
- Persistence owns SQLite/EF observation and database-specific classification;
- `Missing / Uninitialized / Ready / MigrationRequired / TooNew / Unavailable / Invalid` follow the shared Nexus lifecycle vocabulary;
- ambiguous evidence exposes only fact-compatible candidates and a non-authoritative suggestion;
- deterministic evidence such as proven `Ready` exposes a single state;
- structured findings use stable Nexus-compatible `code + details`, including immutable transport-safe primitive collections;
- SQLite integrity remains a factual observation separate from GameSaveSync schema/history coherence;
- classification performs no initialize/migrate/restore/repair/overwrite side effect.

Shared review completed with Damien and covered file responsibilities, execution flow, provider/classifier separation, facts-vs-classification invariants, failure diagnosis and H2.1C explicit deferrals.

NexusPrincipia is the source of truth for durable authorized classification: if a later Admin choice must survive restart, that decision is persisted in trusted control-plane state outside the ambiguous/rejected resource and invalidated/reviewed when material facts change.

## VALIDATED POST-REVIEW HARDENING

The post-H2.1A MachineId hardening is validated locally and remotely.

Validated changes:

- `MachineId.Value` is now `Guid`, rejecting `Guid.Empty`;
- `GameDataRoot` compares MachineId value objects directly;
- obsolete case-insensitive string identity semantics are removed;
- future generation remains deferred to Application/Server enrollment using UUID v7;
- SQLite and Agent do not generate authoritative MachineId values;
- the CI-only missing namespace import in `GameDataRoot.cs` was corrected;
- latest CI for commit `1e9514a` is green.

## DECIDED ARCHITECTURE / REMAINING H2 DIRECTION

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

- post-H2.1A review found that string MachineId semantics could diverge between value equality and case-insensitive duplicate checks;
- approved correction changes MachineId to a non-empty `Guid` value object;
- future new identities are generated as UUID v7 by the authoritative Application/Server enrollment workflow, not by Core, Agent or SQLite;
- Persistence stores the assigned GUID; no database IDENTITY/autoincrement substitutes for MachineId;
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
- persistence entities/mappings live in `GameSave.Persistence`;
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

- database initialization/migration/snapshot/recovery logic is implemented once as `GameSave.Application` use cases/services executed by the Server-side application;
- desktop/local UI, future Web Admin, startup and possible maintenance CLI are only entry-point adapters;
- EF migration classes are authored during development and versioned in Git;
- deployed interfaces may execute already-known migrations but do not dynamically author migration source code;
- startup never applies schema/data migrations automatically; it may inspect/report state only;
- initialization, migration and restore are distinct explicit operations;
- explicit administrative execution is requested through reusable `GameSave.Application` services/use cases shared by Admin, maintenance CLI and future IA/tool adapters;
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

`GameSave.Server` is still a thin ASP.NET Core host:

- no business endpoint;
- no controller;
- no transport DTO;
- no business persistence schema/repositories yet; the validated Persistence foundation now includes metadata DB bootstrap/migrations plus H2.1C read-only inspection;
- no storage backend implementation yet.

`GameSave.Contracts` is still intentionally empty of DTOs until a real boundary requires one.

## EXPECTED TEST WARNINGS

Until H2 adds real Server/Integration behavior, these warnings may still appear and are known:

- `GameSave.Server.Tests`: no tests available;
- `GameSave.IntegrationTests`: no tests available.

As soon as H2 adds real Server behavior, the first warning should naturally disappear because real Server tests should exist. Do not add fake tests.

## SHARED ENGINEERING REFERENCES

Cross-project development rules are now centralized in NexusPrincipia rather than duplicated here.

Authoritative shared references:

- Dev + AI operating model: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/ai-development-operating-model.md
- Project bootstrap: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/project-bootstrap.md
- Session continuity: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/session-continuity.md
- Documentation conventions: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/documentation-conventions.md
- C# / .NET conventions: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/languages/csharp-dotnet.md
- Debug & Observability: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/debug-observability.md
- Entrypoints & reusable operations: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/entrypoints-and-reusable-operations.md
- Database lifecycle/readiness: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/database-lifecycle-readiness.md
- Inspection/classification/authorized choice: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/inspection-classification-authority.md
- Structured inspection findings: https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/structured-inspection-findings.md

GameSaveSync keeps only project-specific workflow rules locally.

## READ FIRST NEXT SESSION

1. root `README.md`;
2. this file;
3. `docs/tranches/H2-central-server.md`;
4. `docs/decisions/ADR-0001-server-metadata-sqlite.md`;
5. `src/GameSave.Server/README.md`;
6. `src/GameSave.Contracts/README.md`;
7. `src/GameSave.Server/Program.cs`;
8. actual remote branch and HEAD.

## H2.1C CLASSIFICATION AUTHORITY — VALIDATED

H2.1C follows the shared Nexus inspection/classification rule: facts → compatible candidates → reasoned suggestion → authorized choice later → separate operation.

GameSaveSync behavior:

- empty valid SQLite with no applied GameSaveSync migration → `Uninitialized | Invalid`, suggest `Uninitialized`;
- valid SQLite with foreign/user tables and no applied GameSaveSync migration → same candidates, suggest `Invalid`;
- only genuinely ambiguous observations expose multiple candidates;
- proven `Ready` / `MigrationRequired` states remain deterministic rather than carrying `Invalid` as a generic rejection option;
- impossible states are never offered;
- inspection explanations use stable `code + details` findings, never authoritative free-form prose;
- existing invalid resources are never silently overwritten.

## NEXT EXACT ACTION

H2.1C is closed. Do not start another implementation slice from assumptions.

Next session:

1. verify remote branch/HEAD;
2. read root README, this handoff and `docs/tranches/H2-central-server.md`;
3. choose and explicitly scope the next H2 slice with Damien;
4. perform the architecture/cadrage discussion before code;
5. preserve the accepted Nexus inspection/classification contracts.

No promotion to `deploy/succumbrae` or `main` has been authorized by H2.1C acceptance.
