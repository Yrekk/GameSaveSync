# ADR-0003 — Metadata recovery uses validated snapshots and restricted mode

**Status:** Accepted  
**Date:** 27 September 2026

## Context

GameSaveSync's central metadata database is safety-significant.

A database failure must not cause the Server to:

- improvise a new empty database;
- silently downgrade schema;
- continue normal synchronization from partially trusted metadata;
- automatically promote an older database snapshot back to authority.

The recovery model follows the same general resilience direction already used for Claviger: keep recoverable snapshots and fall back to a restricted operating mode rather than pretending the database is healthy.

GameSaveSync requires stricter authority rules because restoring older metadata may move central synchronization state backwards relative to Agents or file storage.

## Decision

The Server will maintain SQLite-safe metadata snapshots and support a **restricted recovery/minimal mode**.

A failure affecting the active metadata database does **not** automatically restore a snapshot and resume synchronization.

Conceptually:

```text
active metadata DB
      ↓ failure / incompatibility
restricted recovery mode
      ↓
validated snapshot candidates
      ↓ explicit administrative selection/confirmation
restore candidate
      ↓
database + authority consistency validation
      ↓
normal mode only if validation succeeds
```

## Snapshot policy

Initial direction:

- keep at least the two most recent known-good rolling metadata snapshots;
- create an additional pre-migration snapshot before a schema change that may modify persisted data;
- snapshots must be produced by a SQLite-safe mechanism;
- never rely on a blind copy of a live database file;
- snapshot storage must remain separate from the active database.

Custodia is the intended durable target for backups once the real storage integration exists.

H2 may use local snapshot storage first so the recovery mechanism can be tested before H8 real Custodia integration.

## Restricted recovery mode

If the active metadata database is unavailable, locked beyond policy, corrupt, migration-failed, or schema-incompatible, normal synchronization authority is disabled.

The Server may remain alive only for narrowly scoped recovery/diagnostic capabilities, such as:

- health/status reporting;
- structured diagnostics;
- listing validated snapshot candidates;
- explicit administrative recovery actions.

It must not:

- accept normal PUSH/PULL operations;
- publish new central versions;
- mutate profile/synchronization metadata as though the DB were healthy;
- silently create a replacement authoritative database.

## Snapshot restoration

Snapshot restoration is explicit.

The Server must not automatically choose "latest" and resume normal operation.

A selected snapshot is treated as a recovery candidate until validated.

At minimum, validation must establish:

- the SQLite database can be opened;
- expected schema/migrations are compatible;
- required domain data can be reconstructed without violating Core invariants.

When real central save-version storage exists, recovery validation must also ensure restored metadata is consistent with that authoritative file/version storage before normal synchronization resumes.

This prevents an older metadata snapshot from silently rewinding central authority.

## Migration interaction

A schema migration failure places the Server into restricted recovery mode.

The pre-migration snapshot remains available.

Automatic "Down" migration is not the primary production recovery mechanism.

Recovery prefers:

1. preserve diagnostics;
2. keep the failed DB untouched where practical;
3. select/restore a known-good snapshot explicitly;
4. re-run only known compatible migrations;
5. validate before resuming normal operation.

## Failure posture

The system is fail-closed for synchronization authority.

Availability of diagnostics is preferred over total process death, but diagnostic availability must never be confused with synchronization availability.

## Consequences

Benefits:

- database failures remain diagnosable;
- operator has a controlled recovery path;
- no silent metadata rewind;
- migration failures have a known rollback source;
- resilience pattern stays consistent with the broader Succumbrae tooling direction.

Costs:

- snapshot creation/retention requires explicit implementation and tests;
- recovery mode needs a small separate capability surface;
- later storage integration must reconcile restored metadata with real save-version storage;
- "server process is running" no longer means "synchronization is available", so health/status must expose the distinction clearly.
