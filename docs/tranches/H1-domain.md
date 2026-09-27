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

## H1.1 — current slice

### Scope

Introduce the minimum vocabulary needed by later synchronization rules:

- `SyncVersion`;
- local state with `BaseVersion`, `IsDirty` and `IsGameRunning`;
- central state with `CurrentVersion`.

`SyncVersion` rejects negative values because synchronization versions are monotonic non-negative identifiers.

### Dirty semantics

`IsDirty = true` means local synchronized data changed after the machine's known `BaseVersion`.

It does not mean corruption, and it does not imply that the local copy should automatically replace the central copy.

### Conflict requirement carried forward

When local data is dirty from one base version while the central authority has advanced to another version, the domain must report a conflict rather than selecting a winner automatically.

The V1 conflict flow is manual:

- surface the conflict to the user;
- let the user explicitly choose which side becomes authoritative;
- allow cancellation/no decision;
- preserve both sides before any destructive resolution;
- timestamps may inform the user but never choose the winner automatically.

The actual conflict UI belongs to the replaceable UI tranche; H1 only establishes the domain semantics required to support it safely.

## Tests

H1.1 adds tests only for real invariants introduced by the slice. It does not add placeholder assertions for behavior that does not yet exist.

## Branch promotion

H1 work stops at `develop` after validation and explicit acceptance. No promotion to `deploy/succumbrae` or `main` is implicit.

## Next exact action

Complete H1.1 remote/local validation, then define H1.2's decision result shape before implementing the decision table.
