# H1 — Generic deterministic domain

**Status:** IN PROGRESS  
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

`SyncVersion` rejects negative values because synchronization versions are monotonic non-negative identifiers.

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

Define the explicit decision result shape and implement the deterministic synchronization decision table.

## Next exact action

Design H1.2's decision result shape, then implement and test the decision table.
