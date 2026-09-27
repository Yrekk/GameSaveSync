# H2 — Minimal central server

**Status:** PLANNING / READY FOR DESIGN DISCUSSION  
**Branch:** `feature/h2-central-server`  
**Base:** `develop` after accepted H1 merge

## Objective

Turn the existing ASP.NET Core host into the first real central authority boundary without pulling Agent, Windows lifecycle, real Custodia storage or save-transfer behavior forward.

H2 begins from the accepted H1 domain and ADR-0001 SQLite direction.

## What H2 is allowed to introduce

- Server-side application/persistence boundaries;
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
5. **H2 storage scope** — how much of the validated "fake/local storage" server direction belongs in H2 versus later transactional-transfer/storage tranches?
6. **First real API use case** — what is the smallest server operation worth exposing before an Agent exists?
7. **Machine metadata** — which machine identity fields are truly required now, and which should wait until the Agent provides a real consumer?

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

Continue the H2 design discussion with H2 storage scope, first real API use case and minimum machine metadata before writing H2.1 persistence code.
