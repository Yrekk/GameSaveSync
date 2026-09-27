# Game profile domain

This folder contains the pure domain definition of a synchronizable game profile.

A `GameProfile` is always structurally valid. UI drafts, incomplete forms and partially entered configuration are not `GameProfile` instances and must stay outside the domain until validation succeeds.

## Stable identity

`ProfileId` is a normalized lowercase slug such as:

```text
project-zomboid
space-engineers-2
```

It is intended to remain stable for the lifetime of the profile and may later be used by persistence, logs and storage layout. Display names may change independently.

Core validates the identifier shape. Profile and data-root identifiers are immutable reference value objects rather than structs, so a default struct value cannot bypass their constructors. Global uniqueness is a persistence/repository invariant and will be enforced by the central server database.

## Process names

A profile contains at least one process name.

Process names are stored as configuration only. Core does not inspect Windows processes.

Duplicate process names are rejected case-insensitively.

## Data roots

A profile contains at least one logical `GameDataRoot`.

Each root has:

- a stable logical `DataRootId`;
- a default path expression;
- optional per-machine path overrides keyed by the stable `MachineId` value object.

Core stores these path strings but does not inspect the filesystem or require the path to exist.

Data-root IDs must be unique within a profile. A single data root cannot define multiple overrides for the same machine. Hostname, username and local path changes do not redefine machine identity.

## Exclusions

Exclusions are stored as distinct non-empty expressions.

H1.3 does not define glob syntax or matching semantics. Interpretation belongs to later monitoring/file-selection work.

## Retention

`VersionRetention` controls normal validated central-version history and must be greater than zero.

Managed recovery checkpoint retention is separate.

## Recovery policy

Managed recovery is optional per profile:

```text
Disabled

or

ManagedCheckpoints
├── CheckpointInterval > 0
└── RetentionCount >= 2
```

The minimum managed retention of two preserves the current and previous recovery candidate.

The policy is configuration only in H1.3. No checkpoint capture, transfer or restoration occurs in Core.

## Persistence boundary

Core has no SQLite dependency.

The central server will persist valid profiles and related metadata through a repository boundary. SQLite is the accepted server-side persistence direction, documented by ADR-0001.

Save payloads, central versions, recovery checkpoints and quarantine data remain file/storage concerns rather than database blobs.
