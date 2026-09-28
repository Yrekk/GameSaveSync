# Current handoff

**Date:** 28 September 2026  
**Repository:** `Yrekk/GameSaveSync`  
**Working branch:** `feature/h2-central-server`  
**Integration branch:** `develop`  
**Deployment branch:** `deploy/succumbrae`  
**Stable branch:** `main`  
**Current tranche:** H2 — Minimal central server  
**State:** implementation complete and remote CI green; awaiting final local validation, targeted code review and Damien's explicit H2 acceptance

Always verify the actual remote branch and HEAD before modifying the repository.

## Current remote reference

Final H2 implementation candidate:

```text
f0c8c1ca956abb3e9932965271e4ea1fb5667121
```

CI:

```text
run 36424017414 — SUCCESS
Release build: 0 warnings / 0 errors

GameSave.Core.Tests        : 57 passed
GameSave.Application.Tests : 39 passed
GameSave.Persistence.Tests : 35 passed
GameSave.Storage.Tests     : 7 passed
GameSave.Server.Tests      : 1 passed
GameSave.IntegrationTests  : 1 passed
TOTAL                      : 140 passed
```

The old "no tests available" warnings for Server.Tests and IntegrationTests are no longer expected: both projects now contain real executable tests.

## H2 result

H2 turns the ASP.NET host into the first real central GameSaveSync authority boundary while keeping later Agent/transfer/Custodia work out of scope.

### Metadata database foundation

Implemented:

- explicit configured SQLite metadata path;
- normal operational access cannot silently create a missing DB;
- explicit initialization semantics remain distinct from migration and restore;
- versioned EF migrations;
- read-only database inspection;
- observed facts separated from semantic classification;
- durable Admin classification outside the inspected DB;
- human-readable XML control-plane history;
- fresh inspection revision check before durable human decisions;
- metadata readiness modes: `Normal / Maintenance / RestrictedRecovery / OutOfService`;
- fail-closed behavior without equating readiness failure to Server process death.

### GameProfile persistence

Implemented:

- complete valid `GameProfile` persistence behind an Application repository port;
- migration `20260928123000_AddGameProfiles`;
- global `ProfileId` persistence key;
- human-readable `DisplayName` duplicated beside the serialized aggregate for diagnostics/listing;
- complete aggregate stored as a versioned JSON persistence document;
- Persistence DTO/mapping remains outside Core;
- repository round-trip and update tests.

The JSON aggregate is intentionally not split into speculative relational tables because H2 has no use case that needs SQL queries over every nested profile field yet.

### Metadata snapshots and administrative operations

Implemented Application use cases:

- `InitializeMetadataDatabase`;
- `ApplyPendingMetadataDatabaseMigrations`;
- `CreateRollingMetadataDatabaseSnapshot`;
- `RestoreMetadataDatabaseSnapshot`.

Implemented Persistence infrastructure:

- SQLite-safe backup through `BackupDatabase`;
- validated snapshot candidates;
- rolling retention = 2;
- mandatory pre-migration snapshot;
- explicit snapshot id for restore;
- no automatic "restore newest";
- active broken DB quarantined during restore;
- post-operation reinspection;
- no automatic startup initialization/migration/restore.

Important Windows/SQLite detail:

- snapshot source/destination/validation connections intentionally avoid pooling where file replacement/rotation is involved;
- restore clears only the active metadata connection pool, never `ClearAllPools()`.

### Save artifact Storage boundary

Implemented:

- `IGameSaveArtifactStorage` Application port;
- local filesystem backend in `GameSave.Storage`;
- artifact identity includes profile + logical artifact + data root + relative path;
- basic write/read/existence/status behavior;
- atomic temp-file write before final replace;
- path traversal rejected (`..`, absolute paths, etc.);
- Storage remains separate from EF/SQLite Persistence;
- no H5 reliable publication/version protocol yet;
- no H8 Custodia/NAS backend yet.

Storage composition does not create/touch the root path at Server startup. A broken storage target must remain diagnosable through system status instead of crashing the process before diagnostics are available.

### First real transport boundary

Implemented:

```text
GET /api/system/status
```

Flow:

```text
HTTP
→ GameSave.Server adapter
→ GetSystemStatus
→ metadata inspect + authorized classification
→ metadata readiness
→ recovery snapshot availability
→ storage readiness
→ GameSave.Contracts DTO
```

The response exposes:

- overall operational mode;
- synchronization availability;
- effective metadata state;
- metadata operational mode;
- Admin-classification requirement;
- migration pending signal;
- snapshot recovery availability;
- Storage status;
- structured finding codes.

Process liveness remains distinct from operational readiness.

A real integration test boots the ASP.NET application with isolated temporary paths and calls `GET /api/system/status`.

## Explicitly still deferred after H2

H2 does **not** implement:

- Windows Agent behavior;
- machine enrollment/registry workflow;
- process detection;
- save-folder monitoring;
- Windows lifecycle handling;
- transactional transfer/version publication;
- conflict transfer protocol;
- real Custodia storage;
- Project Zomboid-specific profile behavior;
- Web/Admin UI;
- transport authentication/authorization for destructive administrative operations;
- managed game-save recovery checkpoints.

These belong to later tranches.

## Shared development workflow

NexusPrincipia was updated during H2.

Current shared rule:

- one functional tranche = one dedicated development session;
- internal technical checkpoints are allowed and encouraged;
- checkpoints are not mini-tranches and do not each require acceptance/local validation/document closure;
- pause implementation only for real architectural/product/safety decisions;
- continuous CI/testing during the tranche;
- final review focuses on tricky/important code instead of exhaustive questionnaires;
- explicit local validation + human acceptance close the functional tranche;
- next functional tranche starts in a new session.

Authoritative reference:

`NexusPrincipia/docs/development/ai-development-operating-model.md`

## Final H2 review — files worth opening

Do not review every changed file line-by-line.

The useful technical review is concentrated here:

### 1. Versioned aggregate persistence

```text
src/GameSave.Persistence/Profiles/GameProfileDocument.cs
src/GameSave.Persistence/Profiles/EfGameProfileRepository.cs
```

Topics:

- why Persistence owns the serialization DTO;
- why the stored aggregate has a schema version;
- why `ProfileId` / `DisplayName` are duplicated outside the payload;
- why nested profile fields were not prematurely normalized into multiple SQL tables.

### 2. SQLite snapshot / restore

```text
src/GameSave.Persistence/Database/SqliteMetadataDatabaseSnapshotStore.cs
```

Topics:

- `BackupDatabase` instead of copying a live SQLite file blindly;
- validation before restore;
- explicit snapshot selection;
- broken active DB quarantine;
- targeted pool handling;
- why snapshot connections avoid pooling on Windows.

### 3. Storage path safety

```text
src/GameSave.Application/Storage/SaveArtifactKey.cs
src/GameSave.Storage/Local/LocalGameSaveArtifactStorage.cs
```

Topics:

- provider-neutral artifact key;
- rejection of absolute/traversal paths;
- second containment check after `Path.GetFullPath`;
- atomic temp write.

### 4. Whole-system readiness

```text
src/GameSave.Application/SystemStatus/GetSystemStatus.cs
src/GameSave.Server/Program.cs
src/GameSave.Server/SystemStatus/SystemStatusContractMapper.cs
```

Topics:

- metadata authority alone is not enough for synchronization availability;
- recovery availability participates in metadata readiness;
- Storage participates in global readiness;
- HTTP adapter maps to transport DTO and owns no business policy;
- Server startup still performs no implicit DB lifecycle mutation.

## NEXT EXACT ACTION

Damien performs final local validation of H2 candidate `f0c8c1ca956abb3e9932965271e4ea1fb5667121`.

Recommended local commands:

```powershell
git pull

git branch --show-current
git rev-parse HEAD
git merge-base --is-ancestor f0c8c1ca956abb3e9932965271e4ea1fb5667121 HEAD
$LASTEXITCODE
git status --short

dotnet restore GameSaveSync.sln
dotnet build GameSaveSync.sln --configuration Release --no-restore
dotnet test GameSaveSync.sln --configuration Release --no-build

git diff --check
git status --short
```

Expected:

```text
branch = feature/h2-central-server
$LASTEXITCODE = 0
(the implementation candidate f0c8c1ca... is included in the current HEAD; later documentation-only commits are allowed)

57 Core
39 Application
35 Persistence
7 Storage
1 Server
1 Integration
= 140 tests passed
```

After the automated validation, perform one manual status smoke test:

Terminal 1:

```powershell
dotnet run --project src/GameSave.Server
```

Terminal 2:

```powershell
Invoke-RestMethod http://localhost:5080/api/system/status | ConvertTo-Json -Depth 5
```

With a fresh local Development data directory and no initialized metadata DB, the expected broad behavior is:

- HTTP 200;
- overall mode `Maintenance`;
- synchronization unavailable;
- metadata state `Missing`;
- Storage status `Ready`.

Then stop the Server with Ctrl+C.

After Damien confirms local validation, perform the short targeted code review above. Apply any resulting structural correction, revalidate if code changes, then Damien explicitly accepts H2.

Only after H2 acceptance:

- update/finalize H2 documentation;
- promote H2 to `develop` if Damien explicitly authorizes it;
- start **H3 in a new dedicated session**.

No promotion to `deploy/succumbrae` or `main` is authorized by H2 completion.
