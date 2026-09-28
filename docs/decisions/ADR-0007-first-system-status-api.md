# ADR-0007 — First transport use case is read-only system status

**Status:** Accepted  
**Date:** 27 September 2026

## Context

H2 needs one real transport use case so the Contracts, Server, Application, Persistence and Storage boundaries are exercised end-to-end before an Agent exists.

The first endpoint should provide real operational value without prematurely exposing destructive administration or save-transfer behavior.

GameSaveSync also distinguishes:

- process liveness;
- operational readiness;
- synchronization authority.

A Server process may be alive while the system is in restricted recovery mode and therefore unable to synchronize safely.

## Decision

The first real application/transport use case is:

`GetSystemStatus`

The initial HTTP endpoint is:

`GET /api/system/status`

It is read-only.

The use case is implemented in `GameSave.Application`; the ASP.NET Core endpoint is only an adapter.

`GameSave.Contracts` receives the first real network DTOs needed to represent this status response.

## Initial status responsibility

The response should expose enough information to answer:

- is the Server in normal or restricted recovery mode?
- is metadata persistence healthy?
- are known migrations pending or failed?
- is the configured storage backend healthy enough for H2 operations?
- is synchronization authority currently available?

The exact DTO shape remains implementation detail for H2.4 and should not expose EF entities or Core domain objects directly.

Conceptually:

```json
{
  "mode": "normal",
  "synchronizationAvailable": true,
  "database": {
    "status": "healthy",
    "pendingMigrations": 0
  },
  "storage": {
    "status": "healthy"
  }
}
```

## Liveness is separate

HTTP reachability or process liveness must not be treated as proof that GameSaveSync is operational.

A later liveness endpoint/probe may answer only whether the process is running.

`/api/system/status` answers application readiness/authority.

Example:

```text
Process alive: yes
Mode: recovery
Synchronization available: no
```

## Reuse

The same `GetSystemStatus` application use case is intended to serve:

- desktop/local UI;
- future Web Admin;
- diagnostics/operations;
- later Agent readiness checks when appropriate.

No interface reimplements status logic.

## Scope exclusions

This decision does not expose in H2:

- database restore;
- migration execution;
- profile mutation;
- save PUSH/PULL;
- destructive storage operations;
- Admin authorization model.

Those capabilities require their own explicit transport/security decisions.

## Consequences

Benefits:

- first end-to-end architecture test is low risk;
- Contracts gains a real reason to exist;
- recovery mode becomes externally observable;
- desktop and future Admin share the same status semantics;
- H2 integration tests can cross Server → Application → Persistence/Storage without save-transfer complexity.

Costs:

- status semantics become a maintained contract;
- health/readiness terminology must remain precise as the system grows.
