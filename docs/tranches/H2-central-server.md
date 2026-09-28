# H2 — Minimal central server

**Status:** H2.1C ACCEPTED — H2.1D IMPLEMENTED CANDIDATE — CI GREEN — AWAITING REVIEW / LOCAL VALIDATION / ACCEPTANCE  
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

## Validated implementation slice — H2.1A modular persistence foundation

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

## Validated implementation slice — H2.1B metadata database bootstrap

H2.1B builds the first real SQLite/migration plumbing without introducing business tables or automatic migration execution.

Scope:

- resolve and validate an explicit metadata database path;
- configure `GameSaveDbContext` through Persistence;
- keep normal operational SQLite connections in existing-database-only mode so a missing DB is never silently created;
- provide a separate, explicitly named initialization connection mode that may create the initial database only when a human-authorized initialization operation eventually uses it;
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

## Validated implementation slice — H2.1C metadata database inspection

H2.1C applies the shared NexusPrincipia database lifecycle/readiness model without implementing administrative mutation yet.

Shared references:

https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/database-lifecycle-readiness.md

https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/inspection-classification-authority.md

https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/structured-inspection-findings.md

### Objective

Provide one reusable, read-only application capability that answers:

```text
What is the current technical state of the GameSaveSync metadata database?
```

without initializing, migrating, restoring or repairing it.

### GameSaveSync state vocabulary

The shared baseline is adopted:

```text
Missing
Uninitialized
Ready
MigrationRequired
TooNew
Unavailable
```

GameSaveSync adds:

```text
Invalid
```

`Invalid` means the database is not accepted as usable GameSaveSync metadata authority. The inspector suggests it for SQLite/GameSaveSync inconsistency; an authorized Admin may also select it from a fact-compatible candidate set to reject an otherwise technically compatible database. It is a classification, not a restore/delete/overwrite decision.

### Application boundary

Implemented Application concepts:

- `MetadataDatabaseState` — semantic lifecycle vocabulary;
- `MetadataDatabaseInspectionFacts` — immutable technical observations;
- `MetadataDatabaseInspection` — facts + findings + candidate states + suggestion;
- `InspectionFinding` — Nexus-compatible machine-readable `code + details`;
- `MetadataDatabaseFindingCodes` — canonical shared database finding identifiers;
- `IMetadataDatabaseInspectionProvider` — read-only infrastructure port;
- `InspectMetadataDatabase` — reusable Application use case for future startup/system-status/Admin/CLI/IA adapters.

Application does not depend on EF Core, SQLite types or PRAGMA details.

### Persistence responsibility

The validated Persistence implementation inspects:

- whether the configured DB exists;
- whether it can be opened/read;
- SQLite integrity/consistency signal appropriate for runtime inspection;
- applied EF migration history;
- known pending migrations;
- whether the DB contains migrations newer than the current binary understands;
- whether the observed schema/history is internally inconsistent.

The inspection connection must preserve the H2.1B rule: it cannot create a missing database.

### Explicitly not in H2.1C

- database initialization;
- migration execution;
- snapshot creation/rotation;
- snapshot restore;
- restricted-recovery state machine;
- Admin/CLI/HTTP action surface;
- `GET /api/system/status` transport;
- profile/business persistence schema.

### State / action separation

H2.1C reports state only.

Examples:

```text
Missing
≠ initialize automatically

MigrationRequired
≠ migrate automatically

Invalid
≠ restore automatically
```

The later administrative layer will derive which explicit actions may be offered for each state and will continue to require an authorized choice.

### Tests

Tests must cover the state matrix and forbidden side effects, including:

- Missing without DB creation;
- Uninitialized;
- Ready;
- MigrationRequired;
- TooNew;
- Unavailable;
- Invalid;
- inspection never calling initialization/migration/restore;
- state classification remaining deterministic on reopen.

### Classification authority decision — RESOLVED

The inspector proposes; it never owns the administrative classification.

For an existing valid SQLite database with no applied GameSaveSync migration:

- empty database → candidates `Uninitialized | Invalid`, suggestion `Uninitialized`;
- user/foreign tables present → candidates `Uninitialized | Invalid`, suggestion `Invalid`.

The Admin may override the suggestion only with another fact-compatible candidate. The choice does not mutate the database. Later action availability derives from observed facts + authorized classification + policy, and an existing `Invalid` file must never be silently overwritten.

This is now a shared NexusPrincipia architecture rule.

### H2.1C validation and review

H2.1C is explicitly accepted by Damien on 28 September 2026 after CI, local validation and shared pedagogical review.

Final code HEAD before documentation closure:

```text
d7898c5f2c9d8e96dd23940d3490dbdac65f083c
```

Final CI for that code HEAD:

```text
run 36407964589 — SUCCESS
Release build: 0 warnings / 0 errors
GameSave.Core.Tests        : 57 passed
GameSave.Application.Tests : 7 passed
GameSave.Persistence.Tests : 24 passed
GameSave.Storage.Tests     : 1 passed
TOTAL                      : 89 passed
```

`GameSave.Server.Tests` and `GameSave.IntegrationTests` still intentionally contain no executable tests, so their two no-test notices remain expected.

Review outcomes incorporated before acceptance:

- `InspectionFinding.details` is aligned with the Nexus transport contract, including primitive collections copied into read-only storage;
- proven `Ready` remains deterministic and does not expose `Invalid` as a generic rejection choice;
- SQLite integrity facts are preserved independently from GameSaveSync schema/history coherence;
- provider responsibilities (I/O and observed facts) remain separate from classifier responsibilities (semantic candidates/findings/suggestion);
- CI success is not treated as acceptance; local validation and explicit Damien approval completed the tranche.

H2.1C remains inspection-only. Durable `AuthorizedClassification` persistence is a later capability and, per Nexus, must live outside an ambiguous/rejected inspected resource.

## Active implementation slice — H2.1D metadata readiness and capability policy

H2.1D turns the validated H2.1C inspection result into a deterministic, fail-closed operational/readiness view without executing any administrative mutation.

### Objective

Answer:

```text
Given the current metadata-database inspection,
what operational mode is safe and which capabilities may be considered?
```

### Operational modes

H2.1D introduces four metadata-authority modes:

```text
Normal
Maintenance
RestrictedRecovery
OutOfService
```

Semantics:

- `Normal` — metadata authority is trusted enough for normal operation.
- `Maintenance` — normal authority is unavailable, but the observed condition is expected/administratively actionable, for example first-time initialization or pending migration.
- `RestrictedRecovery` — normal authority is unsafe; only status/diagnostic/recovery-oriented behavior may remain.
- `OutOfService` — the Server process remains alive for status/diagnostics, but no currently known safe maintenance or recovery path is available.

`OutOfService` does **not** mean process death.

### Recovery availability distinction

Snapshot discovery/validation is not implemented in H2.1D.

The readiness policy therefore distinguishes recovery availability conceptually:

```text
Unknown
Available
Unavailable
```

For states requiring recovery:

- `Unknown` → `RestrictedRecovery` without claiming restore is available;
- `Available` → `RestrictedRecovery` and recovery capabilities may be allowed by policy;
- `Unavailable` → `OutOfService`.

This lets later snapshot infrastructure provide real recovery evidence without changing the operational-mode vocabulary.

### Initial readiness direction

```text
Ready
→ Normal
→ metadata authority available
→ later system synchronization availability still depends on Storage/other readiness dimensions

Missing
→ Maintenance
→ explicit initialization may be policy-allowed
→ never initialize automatically

MigrationRequired
→ Maintenance
→ explicit migration may be policy-allowed
→ never migrate automatically

ambiguous CandidateStates without authorized classification
→ Maintenance
→ RequiresAdministratorClassification = true
→ no normal authority

Invalid / TooNew / Unavailable
→ RestrictedRecovery while recovery availability is unknown/available
→ OutOfService when recovery is known unavailable
→ no normal authority
```

H2.1D exposes readiness-safe capabilities only. Final executable/authorized capabilities remain the intersection of readiness policy, implementation availability, operation-specific preconditions and later authorization policy.

### Explicitly not in H2.1D

- persistence of `AuthorizedClassification`;
- Admin classification UI/HTTP/CLI flow;
- database initialization execution;
- migration execution;
- snapshot creation, discovery, validation or rotation;
- snapshot restore;
- restricted-recovery operation coordinator;
- `GET /api/system/status`;
- profile/business persistence schema;
- save-payload storage implementation.

H2.1D is policy/readiness only. Classification, capability availability and operation execution remain distinct layers.

### H2.1D implementation candidate

Implemented in `GameSave.Application`:

- `MetadataDatabaseOperationalMode`;
- `MetadataRecoveryAvailability`;
- `MetadataDatabaseCapability`;
- `MetadataDatabaseReadiness`;
- `MetadataDatabaseReadinessFindingCodes`;
- `EvaluateMetadataDatabaseReadiness`.

The evaluator is pure Application policy: it consumes H2.1C inspection + abstract recovery availability and performs no infrastructure call or mutation.

CI candidate validation:

```text
run 36412100144 — SUCCESS
Release build: 0 warnings / 0 errors
GameSave.Core.Tests        : 57 passed
GameSave.Application.Tests : 22 passed
GameSave.Persistence.Tests : 24 passed
GameSave.Storage.Tests     : 1 passed
TOTAL                      : 104 passed
```

`GameSave.Server.Tests` and `GameSave.IntegrationTests` still intentionally have no executable tests.

Candidate remains unaccepted until shared pedagogical review + Damien local validation.

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
2. **Migrations — RESOLVED:** EF migration code is authored during development, reviewed and versioned in Git. Runtime execution is always an explicit administrative action through a reusable Application use case shared by desktop/API, maintenance CLI and future Web Admin/IA entry points. Startup may inspect/report migration state but never applies migrations automatically. Initialization, migration and restore remain distinct operations. Pre-migration snapshot, application, validation and recovery-mode failure handling are one implementation, never UI-specific. See ADR-0004.
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

Implement H2.1D exactly as scoped above:

1. add provider-neutral readiness/mode/capability models in `GameSave.Application`;
2. derive readiness from H2.1C inspection without mutation;
3. cover the state/recovery matrix with focused Application tests;
4. run CI and perform the mandatory pedagogical review before acceptance.

Do not implement classification persistence, database mutations, snapshots, HTTP transport or business persistence in H2.1D.

No promotion to `deploy/succumbrae` or `main` is authorized.
