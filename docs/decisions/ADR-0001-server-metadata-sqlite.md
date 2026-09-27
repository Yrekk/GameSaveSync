# ADR-0001 — Central server metadata uses SQLite

**Status:** Accepted  
**Date:** 27 September 2026

## Context

The validated technical solution initially allowed atomic JSON metadata and deferred SQLite until the need was demonstrated.

That need is now concrete.

GameSaveSync is expected to maintain relational and mutable metadata including:

- game profiles;
- process names;
- multiple logical data roots;
- per-machine path overrides;
- exclusion rules;
- recovery policies;
- registered machines;
- central synchronization metadata;
- future Admin-console configuration and operational state.

Persisting these relationships as a growing set of JSON files would amount to building an ad-hoc database layer with custom consistency, indexing and migration behavior.

## Decision

GameSaveSync will use **one central SQLite database for server-side configuration and metadata**.

It is not one database per game profile.

Expected deployment direction:

```text
Succumbrae local storage
└── gamesavesync.db
```

A concrete path such as `/srv/succumbrae/data/gamesavesync.db` may be selected during H2 deployment work.

The active SQLite database lives on Succumbrae local storage, not on the Custodia SMB share.

Custodia remains the durable file-storage target for:

- validated save versions;
- staging;
- managed recovery checkpoints;
- quarantine/recovery material;
- safe backups of server metadata/database.

Database backup to Custodia must later use a SQLite-safe backup/snapshot mechanism rather than blindly copying a live database file.

## Domain boundary

`GameSave.Core` does not reference SQLite.

Core defines valid domain objects and invariants.

The Server will later provide a repository/persistence implementation that maps valid domain objects to SQLite.

Conceptually:

```text
Core domain
    ↑
Server application/repository boundary
    ↑
SQLite persistence implementation
```

Global `ProfileId` uniqueness belongs to this central persistence boundary. Core validates identifier format but cannot establish uniqueness across stored profiles by itself.

## Profile identity

A profile uses a stable normalized slug such as `project-zomboid`.

The display name may change independently.

The profile identifier is intended to remain immutable after creation and will be used as a stable persistence/log/storage key.

## Consequences

Benefits:

- transactional metadata updates;
- explicit schema migrations;
- enforceable uniqueness and relational constraints;
- cleaner Admin-console queries later;
- simpler maintenance than distributed JSON metadata;
- local database operation remains available when Custodia/SMB is temporarily unavailable.

Costs:

- H2 must introduce schema/versioning and repository code;
- database backups require SQLite-aware handling;
- persistence migrations become a maintained compatibility surface.

Save payloads remain files. SQLite is not used as a blob store for game saves.
