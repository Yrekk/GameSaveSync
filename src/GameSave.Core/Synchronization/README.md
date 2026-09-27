# Synchronization domain

This folder contains pure synchronization concepts and rules.

It may describe versions, local/central state and deterministic synchronization decisions. It must not contain filesystem access, process detection, HTTP, NAS access, persistence or UI behavior.

## Synchronization version semantics

A `SyncVersion` always represents a real published central version and is strictly positive.

```text
v1
v2
v3
...
```

Version `0` is not a sentinel.

Absence of a version is represented explicitly:

- `LocalSyncState.BaseVersion = null`: the local state has never been based on a published central version;
- `CentralSyncState.CurrentVersion = null`: no central version has been published yet.

This keeps "no version exists" distinct from every real version.

Initial publication is therefore modeled without inventing a fake version:

```text
BaseVersion = none
CentralVersion = none
Dirty = true
→ Push
→ later publication creates v1
```

If a local machine claims a known base version while the central authority has no version, the assessment is `InconsistentState`.

## Dirty state

A local `Dirty` state means that local data changed after the machine's known `BaseVersion`.

It does not mean corruption and does not by itself determine which copy should win.

## Save integrity state

Integrity is modeled independently from `Dirty`.

- `Unknown`: no integrity conclusion is available; this is the safe default.
- `Trusted`: no known integrity concern blocks normal synchronization.
- `RequiresValidation`: something such as an abnormal game/session termination means the save must be checked before it may be promoted centrally.
- `Invalid`: the save has been confirmed unusable.

`Trusted` does not mean the application has mathematically proven that every game file is valid. It means there is currently no known reason to block normal synchronization.

Only a trusted local save may eventually be eligible for automatic central promotion. The exact decision rule is implemented in H1.2.

Timestamps may later be exposed as diagnostic information, but they must not select a winner during a conflict.

## H1.2 synchronization assessment

Normal synchronization is evaluated as a complete deterministic assessment:

```text
LocalSyncState
+
CentralSyncState
    ↓
SyncAssessment
├── Disposition
└── Findings
```

The disposition preserves the underlying local/central relationship.

For states with published versions:

- same base + clean → `Nothing`;
- central ahead + clean → `Pull`;
- same base + dirty → `Push`;
- central ahead + dirty → `Conflict`;
- central behind the local base → `InconsistentState`.

For explicit no-version states:

- no local base + no central version + clean → `Nothing`;
- no local base + no central version + dirty → `Push` (initial publication candidate);
- no local base + central version + clean → `Pull`;
- no local base + central version + dirty → `Conflict`;
- known local base + no central version → `InconsistentState`.

`SyncAssessment` is an immutable reference result rather than a struct so an accidental default value cannot silently look like `Nothing + None`.

Findings are independent and cumulative. A running game or a local integrity problem does not hide the underlying disposition.

For example:

```text
BaseVersion = 42
Dirty = true
CentralVersion = 43
GameRunning = true
Integrity = RequiresValidation

→ Disposition = Conflict
→ Findings = GameRunning + LocalSaveRequiresValidation
```

The normal synchronization executor must later respect these findings before performing destructive work. H1.2 only assesses; it does not execute.

Managed recovery checkpoints are evaluated separately and may later be presented by the UI alongside this assessment.
