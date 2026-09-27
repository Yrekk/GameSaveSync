# GameSave.Core

Pure domain layer for GameSaveSync.

## Boundary

This project stays independent from Windows APIs, ASP.NET Core, NAS access, concrete persistence and UI technology.

H1 introduces deterministic synchronization vocabulary and rules here. Infrastructure components may provide inputs to Core, but they do not own or duplicate the synchronization policy.

Current domain work lives under [Synchronization](Synchronization/README.md) and [Profiles](Profiles/README.md).

## Diagnostics boundary

Core does not write directly to the console, log files or the future Admin diagnostics stream.

Core returns explicit domain states and decisions. The infrastructure layer that calls Core is responsible for turning those results into structured operational events.

Conceptually:

```text
Core decision/state
        ↓
Agent / Server
        ↓
structured diagnostics
   ├── local console
   ├── local rotating logs
   └── live Admin stream
```

The Admin interface must not scrape process stdout. Console output, persistent logs and live diagnostics are different renderings/transports of the same structured diagnostic information.

Normal remote diagnostics are intended to expose `Information` through `Critical`. `Debug` and `Trace` remain local by default and may later be enabled temporarily from Admin with an automatic expiry.
