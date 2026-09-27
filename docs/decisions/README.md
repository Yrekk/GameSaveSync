# Architectural decisions

This directory is reserved for ADR-style records when a decision is significant, non-obvious or costly to reverse.

Do not create an ADR for every class, DTO, package or routine implementation choice.

H0 contains no ADR because the current boundaries come directly from the validated technical solution and do not yet require an additional decision record.

## Accepted records

- [ADR-0001 — Central server metadata uses SQLite](ADR-0001-server-metadata-sqlite.md)

- [ADR-0002 — EF Core is the default persistence layer over SQLite](ADR-0002-ef-core-sqlite-persistence.md)

- [ADR-0003 — Metadata recovery uses validated snapshots and restricted mode](ADR-0003-metadata-snapshot-recovery.md)
