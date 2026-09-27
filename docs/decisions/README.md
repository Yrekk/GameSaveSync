# Architectural decisions

This directory is reserved for ADR-style records when a decision is significant, non-obvious or costly to reverse.

Do not create an ADR for every class, DTO, package or routine implementation choice.

H0 contains no ADR because the current boundaries come directly from the validated technical solution and do not yet require an additional decision record.

## Accepted records

- [ADR-0001 — Central server metadata uses SQLite](ADR-0001-server-metadata-sqlite.md)

- [ADR-0002 — EF Core is the default persistence layer over SQLite](ADR-0002-ef-core-sqlite-persistence.md)

- [ADR-0003 — Metadata recovery uses validated snapshots and restricted mode](ADR-0003-metadata-snapshot-recovery.md)

- [ADR-0004 — Administrative operations are reusable application use cases](ADR-0004-reusable-administrative-use-cases.md)

- [ADR-0005 — Application and Persistence are separate extractable modules](ADR-0005-application-persistence-modules.md)

- [ADR-0006 — Save payload storage is a separate module with a local H2 backend](ADR-0006-separate-storage-module.md)

- [ADR-0007 — First transport use case is read-only system status](ADR-0007-first-system-status-api.md)
