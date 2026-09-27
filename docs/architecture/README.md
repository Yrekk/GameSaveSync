# Architecture

H0 defines **boundaries, not business rules**.

## Components

```text
Windows PC(s)
  GameSave.Agent
  GameSave.Agent.UI
        |
        | HTTP/API boundary
        v
GameSave.Server on Succumbrae
        |
        v
Custodia
```

The validated technical solution establishes a central-server architecture with NAS-backed storage.

H0 materializes the server as an ASP.NET Core host because the network/API boundary is already known. It does not define business routes, controllers or synchronization behavior.

## Project boundaries

| Project | Reserved responsibility | H0 rule |
| --- | --- | --- |
| GameSave.Core | Pure domain | No infrastructure dependency |
| GameSave.Contracts | Shared boundary contracts | No DTO until a real contract needs one |
| GameSave.Server | Central ASP.NET Core HTTP host | Host exists; no business API yet |
| GameSave.Agent | Windows-side engine | No monitoring or transfer behavior yet |
| GameSave.Agent.UI | Replaceable local UI | No business logic and no UI framework selected yet |

## Dependency direction

```text
Core       Contracts
  ^           ^
  |           |
  +--- Server |
  +--- Agent -+
         ^
         |
      Agent.UI
```

These references establish the places where later code can live without implementing that code prematurely.

## Architecture versus application behavior

H0 may materialize a technical boundary when that boundary is already decided.

For example, `GameSave.Server` is an ASP.NET Core host because the system is designed around a central HTTP/API server. H0 still avoids deciding endpoint names, controller organization or DTO shapes before real application contracts exist.

## Documentation rule

When later architecture becomes non-obvious, document the **reason** near the relevant boundary. Obvious models and DTOs do not receive commentary merely to increase documentation volume.

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

## Central metadata persistence direction

Server-side configuration and metadata will use one central SQLite database on Succumbrae local storage.

The active database must not live on the Custodia SMB share. Custodia remains the file-storage and backup target.

Core remains persistence-agnostic. SQLite mapping and schema/migration work belong to the Server infrastructure beginning in H2.

See [ADR-0001 — Central server metadata uses SQLite](../decisions/ADR-0001-server-metadata-sqlite.md).
