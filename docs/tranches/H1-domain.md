# H1 — Generic deterministic domain

**Status:** READY FOR ACCEPTANCE  
**Branch:** `feature/h1-domain`  
**Base:** `develop`

## Objective

Build the pure deterministic synchronization domain without filesystem, network, Windows, NAS, persistence or UI behavior.

## Planned slices

- H1.1 — domain vocabulary and synchronization state;
- H1.2 — deterministic synchronization decision engine;
- H1.3 — generic game profile domain and invariants;
- H1.4 — consolidation, tests and documentation.

## H1.1 — VALIDATED

### Scope

Introduce the minimum vocabulary needed by later synchronization rules:

- `SyncVersion`;
- local state with `BaseVersion`, `IsDirty`, `IsGameRunning` and `IntegrityState`;
- central state with `CurrentVersion`.

`SyncVersion` was initially introduced as a non-negative identifier. H1.4 tightens this invariant: a `SyncVersion` now represents only a real published version and is strictly positive; absence is modeled explicitly in local/central state instead of using version `0`.

### Dirty semantics

`IsDirty = true` means local synchronized data changed after the machine's known `BaseVersion`.

It does not mean corruption, and it does not imply that the local copy should automatically replace the central copy.

### Integrity semantics

Save integrity is independent from `Dirty`:

- `Unknown` is the fail-closed default;
- `Trusted` means no known integrity concern blocks normal synchronization;
- `RequiresValidation` means explicit validation is required before central promotion;
- `Invalid` means the save is confirmed unusable.

A likely source of `RequiresValidation` is a game/session that started and modified data but did not record a clean end. Detecting that condition belongs to later Agent/lifecycle work; H1 only establishes the domain state required to represent it safely.

### Mandatory user-validation flow

When an abnormal end makes a dirty save require validation:

1. do not PUSH it and do not promote it as the central `current` version;
2. tell the user that the game does not appear to have closed cleanly;
3. ask the user to launch the game and verify that the save loads correctly;
4. offer explicit outcomes: **OK**, **KO**, or **decide later**;
5. `OK` may return the save to a trusted state and the synchronization decision is then recalculated;
6. `KO` marks the local save invalid and keeps central recovery available;
7. no response leaves synchronization blocked.

A suspect local copy may later be preserved in quarantine for recovery/diagnostics, but it must never silently replace the last trusted central version.

### Conflict requirement carried forward

When local data is dirty from one base version while the central authority has advanced to another version, the domain must report a conflict rather than selecting a winner automatically.

The V1 conflict flow is manual:

- surface the conflict to the user;
- let the user explicitly choose which side becomes authoritative;
- allow cancellation/no decision;
- preserve both sides before any destructive resolution;
- timestamps may inform the user but never choose the winner automatically.

The actual conflict and integrity-validation UI belongs to the replaceable UI tranche; H1 only establishes the domain semantics required to support it safely.

### Future UX note — not H1

A later version may add local TTS/voice/AI assistance so natural-language answers can be mapped to the same explicit validation actions. That layer must remain an interface convenience: it cannot bypass the deterministic domain states or perform a destructive choice without an explicit resolved action.

## Tests

H1.1 adds tests only for real invariants introduced by the slice. The default integrity state is tested because its fail-closed value is safety-significant.

## H1.1 validation

Validated locally by Damien on 27 September 2026. Restore/build/test are green; `GameSave.Core.Tests` discovered and executed the real H1.1 tests successfully. The remaining no-test warnings are expected for the intentionally empty Server/Integration test projects. Remote CI is green.

## Branch promotion

H1 work stops at `develop` after validation and explicit acceptance. No promotion to `deploy/succumbrae` or `main` is implicit.

## H1.2 — next slice

Define the explicit normal synchronization assessment and implement the deterministic Local + Central decision table.

H1.2 must expose all simultaneously detectable synchronization findings instead of hiding them behind a first-match blocker. The intended result shape is an assessment containing:

- the underlying synchronization disposition;
- zero or more findings/conditions that require attention or prevent immediate execution.

Managed recovery checkpoints remain a parallel later subsystem. H1.2 must not require checkpoint state in order to assess normal synchronization. A future UI can combine synchronization findings with recovery options in one user-facing session.

## H1.2 — VALIDATED

H1.2 now models a complete `SyncAssessment` instead of a first-match blocking decision.

The assessment contains:

- `SyncDisposition`: `Nothing`, `Pull`, `Push`, `Conflict` or `InconsistentState`;
- cumulative `SyncFindings`: game running, unknown integrity, validation required, invalid local save, or central version behind the local base.

A running game does not erase the underlying disposition. Example: a dirty local save on the same central version while the game is running remains `Push` + `GameRunning`; normal synchronization must wait, while an optional managed-recovery subsystem may operate independently.

Tests cover the four core disposition cases, cumulative findings, integrity findings, running-game behavior and inconsistent version history.

`SyncFindings` are observations, not only blockers: for example, `Disposition = Nothing` may still legitimately carry `GameRunning`. Later layers decide whether a finding is informational, warning-level or action-blocking.

## H1.2 validation

Validated remotely and locally on 27 September 2026. CI is green; local Release build is green; `GameSave.Core.Tests` reports 15/15 passing tests; `git diff --check` and working-tree status are clean. The expected no-test warnings remain only for the intentionally empty Server/Integration test projects.

## H1.3 — VALIDATED

H1.3 adds a generic profile domain under `GameSave.Core/Profiles`.

A `GameProfile` is always complete and structurally valid. UI drafts and incomplete forms do not enter the domain.

Implemented profile concepts:

- stable normalized `ProfileId` slug;
- display name and enabled state;
- one or more process names;
- one or more logical `GameDataRoot` values;
- optional per-machine root path overrides;
- exclusion expressions stored without defining matching syntax yet;
- validated central-version retention;
- optional `RecoveryPolicy` with managed-checkpoint interval and retention;
- external-cloud warning configuration.

`CentralVersion` is intentionally not stored inside `GameProfile`; runtime synchronization state remains in the synchronization domain.

A profile may be disabled, but it must still be structurally complete. There are no persisted incomplete domain drafts.

SQLite is now the accepted central metadata persistence direction, recorded in ADR-0001. H1.3 does not implement persistence: Core remains independent of SQLite. Global profile-ID uniqueness will be enforced later by the Server repository/database boundary.

### H1.3 invariants

- profile and data-root identifiers are normalized lowercase slugs represented by immutable reference value objects, preventing `default(struct)` from bypassing construction invariants;
- a profile has at least one process name and one data root;
- process names, exclusions and data-root IDs cannot be duplicated;
- one data root cannot define two overrides for the same machine;
- normal version retention is greater than zero;
- managed checkpoint interval is greater than zero;
- managed checkpoint retention is at least two (current + previous);
- no filesystem/process/NAS/database access occurs in Core.

## H1.3 validation

Validated remotely and locally on 27 September 2026. CI is green; local Release build is green; the complete local test run reports 44/44 passing tests; `git diff --check` and working-tree status are clean. The only warnings are the expected no-test warnings for the intentionally empty Server/Integration test projects.

## H1.4 — CONSOLIDATION IN PROGRESS

H1.4 audits the complete H1 domain before H2 infrastructure work begins.

### Explicit missing-version model

The original H1.1 model allowed `SyncVersion(0)`. This is now intentionally removed.

A synchronization version must be strictly positive and is represented by an immutable reference value object. There is no sentinel version.

Absence is explicit:

- `LocalSyncState.BaseVersion = null` means the local state has never been based on a published central version;
- `CentralSyncState.CurrentVersion = null` means no central version has been published.

The deterministic assessment now covers initial publication and missing-version cases:

- no local base + no central + clean → `Nothing`;
- no local base + no central + dirty → `Push`;
- no local base + central exists + clean → `Pull`;
- no local base + central exists + dirty → `Conflict`;
- known local base + missing central → `InconsistentState`;
- central behind known local base → `InconsistentState`.

`LocalBaseVersionMissing`, `CentralVersionMissingForKnownLocalBase` and `CentralVersionBehindLocalBase` provide explicit findings where relevant.

### Default-value hardening

`SyncVersion` is now a reference value object so `default(struct)` cannot bypass the positive-version invariant.

`SyncAssessment` is also an immutable reference result so an accidental default struct cannot masquerade as `Nothing + None`.

The remaining state structs intentionally have safe defaults:

- default local state = no base + not dirty + game stopped + integrity unknown;
- default central state = no published version.

Those defaults map to explicit, fail-closed semantics rather than a hidden sentinel.

### H1.4 validation status

H1.4 checks are green remotely and locally on 27 September 2026:

- final branch CI is green;
- local Release build is green;
- full local test run reports 51/51 passing tests;
- the only warnings are the expected no-test warnings for intentionally empty Server/Integration test projects;
- `git diff --check` is clean;
- local working tree is clean.

### Final H1 audit

The feature branch is ahead of `develop` only and is not behind it.

The H1 diff is limited to:

- pure Core synchronization/profile domain code;
- Core unit tests;
- architecture/tranche/continuity documentation;
- ADR-0001 for the accepted future SQLite metadata direction.

No H2 infrastructure has been pulled forward: no SQLite package/schema/repository, HTTP business API, filesystem/NAS access, Windows process access, save transfer or UI behavior has been introduced.

H1 is therefore technically ready for explicit tranche acceptance.

## Next exact action

Damien explicitly accepts or rejects H1. On acceptance, merge `feature/h1-domain` into `develop` and stop there unless a later deployment promotion is explicitly approved.
