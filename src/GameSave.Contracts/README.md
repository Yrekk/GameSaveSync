# GameSave.Contracts

Reserved for contracts shared across real application/transport boundaries.

## Boundary

This project is not a dumping ground for every model used by more than one project. A type belongs here only when an actual cross-process or API boundary requires a shared contract.

No transport DTO exists yet. The accepted first real transport use case is the future read-only system-status API; its contracts will be introduced only with that implementation.
