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

Runtime diagnostics are a shared structured event stream, not a screen-scrape of process consoles.

The same diagnostic event may be rendered to:

- the local console;
- rotating local log files;
- a live Admin diagnostics stream.

The Admin interface should behave like an operations runtime console: filterable by application/component, machine, profile, severity, event/category, correlation identifier and time window.

Normal live forwarding includes `Information`, `Warning`, `Error` and `Critical`. `Debug` and `Trace` remain local by default.

Admin may temporarily enable Debug mode for a selected application or machine. Remote Debug activation must:

- be explicit;
- have a bounded duration/TTL;
- automatically return to the normal level;
- be auditable;
- never require an application restart when the logging provider supports runtime level changes.

Local persistent logs remain required even when live diagnostics exist, so failures that occur while the server or network is unavailable remain recoverable.

Diagnostics must not expose secrets, authentication material or NAS credentials.
