# Current handoff

**Date:** 27 September 2026  
**Repository:** `Yrekk/GameSaveSync`  
**Working branch:** `feature/h1-domain`  
**Integration branch:** `develop`  
**Deployment branch:** `deploy/succumbrae`  
**Stable branch:** `main`  
**Current tranche:** H1 — Generic deterministic domain

Always verify the actual remote branch and HEAD before modifying the repository.

## VALIDATED

- H0 bootstrap is validated and promoted through `main`.
- H1.1 is validated locally and remotely.
- H1.2 is validated locally and remotely:
  - deterministic `SyncAssessment`;
  - underlying `SyncDisposition`;
  - cumulative `SyncFindings`;
  - 15/15 Core tests locally;
  - Release build, diff-check and working tree clean.
- Branch promotion policy is `feature/* → develop → deploy/succumbrae → main`.
- Starting with H1, validated feature work stops at `develop` unless Damien explicitly approves deployment promotion.

## IMPLEMENTED BUT NOT YET VALIDATED

H1.4 consolidation:

- `SyncVersion` is now a strictly-positive immutable reference value object;
- version `0` is no longer valid or used as a sentinel;
- missing local/central versions are explicit nullable state;
- initial publication and missing-version truth-table cases are implemented and tested;
- known local base + missing central is explicit `InconsistentState`;
- `SyncAssessment` is now a reference result so default struct state cannot masquerade as a valid assessment.

H1.3 remains validated.

## H1.3 VALIDATED DETAILS

H1.3 on `feature/h1-domain`:

- `ProfileId` stable normalized slug as an immutable reference value object;
- `DataRootId` stable logical-root slug as an immutable reference value object;
- `MachinePathOverride` immutable reference object;
- `GameDataRoot` with default path and per-machine overrides;
- `RecoveryPolicy` with disabled/managed-checkpoint modes;
- complete/always-valid `GameProfile`;
- profile-domain invariant tests;
- profile-domain README;
- ADR-0001 accepting central SQLite metadata persistence.

## DECIDED BUT NOT YET IMPLEMENTED

### Persistence

GameSaveSync will use one central SQLite database for server-side configuration and metadata.

- active DB on Succumbrae local storage;
- not hosted live on Custodia/SMB;
- Custodia receives SQLite-safe backups;
- save payloads remain files, not DB blobs;
- Core has no SQLite dependency;
- repository/schema/migration work begins in H2;
- global `ProfileId` uniqueness is a repository/database invariant.

### Recovery

Managed recovery checkpoints remain parallel to normal synchronization.

- opt-in per profile;
- initial Project Zomboid direction: approximately 10-minute interval;
- minimum two rolling checkpoints: current + previous;
- checkpoint carries source machine and base-version context;
- local state is quarantined before recovery restoration;
- cleanup waits for user validation plus successful final central promotion acknowledgement.

See `docs/architecture/recovery-checkpoints.md`.

### Operational diagnostics

Structured runtime diagnostics feed console, local files and a future filterable live Admin stream. Normal remote levels are Information through Critical; Debug/Trace remain local unless temporarily enabled with expiry.

## TESTS / SMOKE

H1.1 and H1.2 are green remotely and locally.

H1.3 remote CI and local validation are green: Release build green, 44/44 tests passing, diff-check clean, working tree clean.

Expected local-test warnings that may be ignored for now:

- `GameSave.Server.Tests`: no tests available;
- `GameSave.IntegrationTests`: no tests available.

These projects are intentionally empty at the current tranche. Do not add fake tests to silence the warnings.

No filesystem, save, NAS, Windows process, network, SQLite or synchronization transfer behavior has been introduced in Core.

H1.4 consolidation validation is pending.

## KNOWN RISKS

- Keep profile configuration generic; no hard-coded Project Zomboid rule may enter Core.
- Do not let UI drafts become persisted `GameProfile` objects.
- Core validates ID shape, but only the future repository/database can enforce global profile-ID uniqueness.
- Recovery configuration must remain separate from actual checkpoint execution/storage.

## READ FIRST NEXT SESSION

1. root `README.md`;
2. this file;
3. `docs/tranches/H1-domain.md`;
4. `src/GameSave.Core/Profiles/README.md`;
5. `src/GameSave.Core/Synchronization/README.md`;
6. `docs/decisions/ADR-0001-server-metadata-sqlite.md`;
7. actual remote branch and HEAD.

## NEXT EXACT ACTION

Validate H1.4 remotely and locally. If green, perform final H1 acceptance/audit before opening H2.
