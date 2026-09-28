# Architecture

GameSaveSync uses a central-server architecture with explicit application, persistence and storage boundaries.

## Components

```text
Windows PC(s)
  GameSave.Agent.UI
        ↓
  GameSave.Agent
        │ HTTP/API
        ▼
GameSave.Server on Succumbrae
        │
        ├── GameSave.Application
        │      ↓
        │   GameSave.Core
        │
        ├── GameSave.Persistence
        │      └── EF Core / SQLite metadata
        │
        └── GameSave.Storage
               └── save payloads / artifacts

Future durable storage target: Custodia
```

A synchronizable profile supports **1 to N machines**. One-machine use is valid for central backup/resilience; multi-machine use adds synchronization between PCs.

## Project boundaries

| Project | Responsibility |
| --- | --- |
| GameSave.Core | Pure domain rules, value objects and deterministic synchronization policy |
| GameSave.Application | Use cases and required capability/repository ports |
| GameSave.Persistence | EF Core/SQLite metadata persistence and database-specific infrastructure |
| GameSave.Storage | Save-payload and file-artifact storage implementations |
| GameSave.Contracts | Real transport/shared boundary contracts only |
| GameSave.Server | ASP.NET Core host, transport adapter and composition root |
| GameSave.Agent | Windows-side synchronization engine when implemented |
| GameSave.Agent.UI | Replaceable local UI adapter |

## Dependency direction

```text
UI / transport / infrastructure
            ↓
       Application
            ↓
          Core
```

Persistence and Storage implement capabilities required by Application. Server wires concrete implementations but does not own domain, persistence or administrative business logic.

## Entrypoint rule

`Program.cs` is a composition/bootstrap boundary.

If an operation may be requested by Admin, a maintenance CLI or a future IA/tool, its implementation belongs to a reusable `GameSave.Application` use case/service.

Startup may inspect state but must not silently choose between legitimate administrative actions such as initialize, migrate, restore or recovery mode.

See the shared [NexusPrincipia entrypoint rule](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/entrypoints-and-reusable-operations.md).

## Central metadata persistence

The active metadata database is SQLite on Succumbrae local storage, never on the Custodia SMB share.

Validated H2.1B rules:

- operational access requires an existing database;
- initial creation is a distinct explicit administrative capability;
- schema changes use versioned EF migrations;
- startup never applies migrations automatically;
- initialize, migrate and restore are distinct decisions;
- EF infrastructure remains internal to Persistence.

Validated H2.1C rules:

- metadata DB inspection is read-only and reusable through Application;
- observed facts are not rewritten to match a semantic classification;
- SQLite integrity (for example `PRAGMA quick_check`) is distinct from GameSaveSync schema/history coherence;
- ambiguous observations expose constrained candidate states plus a suggestion rather than an inspector-owned decision;
- findings are stable machine-readable `code + details` values rendered into human prose only by adapters;
- proven states such as `Ready` remain deterministic; `Invalid` is not a generic rejection button;
- durable future Admin classifications belong to trusted control-plane state outside an ambiguous/rejected inspected resource.

Validated H2.1D rules:

- metadata readiness is derived in Application from inspection + abstract recovery availability;
- operational modes are `Normal`, `Maintenance`, `RestrictedRecovery`, `OutOfService`;
- `MetadataAuthorityAvailable` is narrower than whole-system synchronization availability;
- `SafeCapabilities` express what metadata readiness does not rule out, not user authorization or implementation availability;
- unresolved classification fails closed and exposes classification resolution rather than implicit mutation;
- `OutOfService` is a live diagnostic state, not process death;
- no readiness evaluation performs initialization, migration, restore or any other mutation.

H2.1E candidate rules:

- durable human classification is trusted control-plane state outside the inspected SQLite resource;
- deterministic one-candidate classifications never require or write control-plane decisions;
- ambiguous classifications are reusable only for the same resource identity, inspection revision, policy version and compatible candidate set;
- recording a decision re-inspects first and rejects stale expected revisions;
- the control plane preserves complete history and human-readable label snapshots beside stable identifiers;
- labels are diagnostic only and never become identity/authorization keys;
- malformed/unavailable control-plane state fails closed for ambiguous classification but does not invalidate an otherwise deterministic inspection;
- the XML persistence format is infrastructure-only and does not leak into Application.

Custodia is intended to receive safe backups later.

GameSaveSync follows the shared [NexusPrincipia database lifecycle/readiness reference](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/database-lifecycle-readiness.md), [inspection/classification authority reference](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/inspection-classification-authority.md) and [structured inspection findings reference](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/structured-inspection-findings.md) and [audit identifiers/human-readable labels reference](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/audit-identifiers-and-labels.md).

GameSaveSync-specific extensions are:

- observed database state remains separate from operational mode and available actions;
- the shared state baseline is `Missing`, `Uninitialized`, `Ready`, `MigrationRequired`, `TooNew`, `Unavailable`;
- GameSaveSync adds `Invalid` for proven inconsistency/corruption and as an authorized alternative only inside genuinely ambiguous fact-compatible candidate sets;
- unsafe states fail closed for synchronization authority;
- snapshot/recovery actions are explicit and never selected automatically;
- restricted recovery remains separate from the observed database state.

See ADR-0001 through ADR-0005 for the accepted persistence/application decisions.
## Operational diagnostics direction

GameSaveSync follows the shared NexusPrincipia Debug & Observability reference:

[Debug & observability — NexusPrincipia](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/architecture/debug-observability.md)

GameSaveSync-specific diagnostics should expose relevant context such as application/component, `MachineId`, profile, event/category and correlation identifier when those concepts have a real runtime consumer.

The shared reference owns the cross-application rules for runtime Debug sessions, TTL/expiry, structured logs, secret redaction, local rotating logs and future Admin integration. Those rules are not duplicated here.

## Managed recovery checkpoints

Some games may need an optional recovery layer in addition to normal synchronization.

Managed recovery checkpoints are temporary recovery candidates created while a configured game is running. They are not validated central versions and they never become authoritative merely because capture succeeded.

Normal synchronization and managed recovery remain separate concerns so games with their own adequate autosave/recovery do not inherit unnecessary behavior.

The detailed contract is documented in [managed recovery checkpoints](recovery-checkpoints.md).
