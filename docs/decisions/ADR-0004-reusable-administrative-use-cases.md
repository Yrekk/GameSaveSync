# ADR-0004 — Administrative operations are reusable application use cases

**Status:** Accepted  
**Date:** 27 September 2026

## Context

GameSaveSync will eventually expose administrative operations through more than one interface.

Expected entry points include:

- the desktop/local application used from a fixed PC or laptop;
- the future Web Admin interface;
- server startup/bootstrap logic;
- potentially a maintenance CLI or other operational adapter.

Operations such as database migration, snapshot creation, snapshot restoration, compatibility checks and recovery actions are safety-significant.

Implementing them separately in each interface would create duplicated behavior and allow the desktop UI, Web Admin and startup path to diverge.

## Decision

Administrative operations are implemented once as reusable Server application use cases/services.

Interfaces are adapters only.

Conceptually:

```text
Desktop UI ─────┐
Web Admin ──────┤
Server startup ─┤
Maintenance CLI ┘
        ↓
Application use case / coordinator
        ↓
Persistence + recovery infrastructure
        ↓
EF Core / SQLite / snapshots
```

No UI owns migration, snapshot or recovery business logic.

## Migration distinction

Two different activities must not be confused.

### Migration authoring

EF migration source code is created during development:

```text
dotnet ef migrations add ...
→ review generated migration
→ commit to Git
→ build/deploy with application
```

A production UI does not dynamically author new EF migration classes.

### Migration execution

Applying migrations that are already shipped with the deployed Server is an operational use case.

That use case must be reusable from multiple entry points.

Typical operations include:

- inspect current database/schema state;
- list pending known migrations;
- create/validate a pre-migration snapshot;
- apply known pending migrations;
- validate resulting schema/domain readability;
- expose success/failure diagnostics;
- enter restricted recovery mode on failure;
- list recovery snapshots;
- explicitly restore a selected snapshot;
- revalidate before returning to normal mode.

## Startup reuse

Automatic startup migration must call the same application-level migration coordinator used by administrative entry points.

Startup is not allowed to contain a second private implementation of migration logic.

This prevents:

```text
Program.cs migration logic
!=
Desktop migration logic
!=
Web Admin migration logic
```

## Interface rule

User interfaces may:

- display operation state;
- request an operation;
- ask for explicit confirmation;
- display progress and structured diagnostics.

They may not:

- directly open or mutate the SQLite database;
- call EF Core directly;
- implement snapshot rotation;
- decide migration ordering;
- bypass recovery-mode safety checks.

## Remote operation

When an operation is initiated from a PC or future Web Admin interface, the authoritative operation still executes on the Server/Succumbrae side.

The client requests the use case; it does not manipulate the server database locally.

## Authorization

The mechanism is reusable, but exposure is not automatically unrestricted.

Later transport/Admin work must define authorization for safety-significant operations such as:

- applying migrations;
- restoring snapshots;
- leaving recovery mode.

Reusability does not imply that every client may invoke every operation.

## Consequences

Benefits:

- one implementation to test and maintain;
- identical safety behavior across desktop, Web Admin and startup;
- easier future Admin integration;
- operational logic remains independent from UI technology;
- simpler automation and testing.

Costs:

- application/use-case boundaries must be designed before UI endpoints;
- progress/status reporting may require explicit operation-result models;
- long-running administrative operations may later need job/correlation identifiers.

## Relationship to repository boundaries

This ADR establishes the application-use-case layer but does not yet decide the exact placement or shape of repository interfaces.

That remains an explicit H2 design question.
