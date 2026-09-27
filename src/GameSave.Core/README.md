# GameSave.Core

Pure domain layer for GameSaveSync.

## Boundary

This project stays independent from Windows APIs, ASP.NET Core, NAS access, concrete persistence and UI technology.

H1 introduces deterministic synchronization vocabulary and rules here. Infrastructure components may provide inputs to Core, but they do not own or duplicate the synchronization policy.

Current domain work lives under [Synchronization](Synchronization/README.md).
