# Synchronization domain

This folder contains pure synchronization concepts and rules.

It may describe versions, local/central state and deterministic synchronization decisions. It must not contain filesystem access, process detection, HTTP, NAS access, persistence or UI behavior.

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
