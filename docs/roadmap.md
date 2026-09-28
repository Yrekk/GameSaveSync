# Roadmap

The validated technical solution defines the progression below.

| Tranche | Purpose | State |
| --- | --- | --- |
| H0 | Bootstrap, boundaries, CI and documentation system | Validated |
| H1 | Generic deterministic domain | Accepted / merged to develop |
| H2 | Minimal central server, including SQLite metadata persistence boundary | In progress — H2.1A/H2.1B/H2.1C/H2.1D validated |
| H3 | Minimal Windows agent | Planned |
| H4 | Replaceable minimal UI | Planned |
| H5 | Reliable transactional transfers | Planned |
| H6 | Generic process/folder monitoring | Planned |
| H7 | Windows lifecycle handling | Planned |
| H8 | Real Custodia storage | Planned |
| H9 | First real profile: Project Zomboid | Planned |
| H10 | Second game as genericity proof | Planned |
| H11 | Measured optimization | Planned |

A tranche must not silently pull work forward from a later tranche simply because the future design is already known.

## Cross-cutting operational requirement

A later infrastructure/admin tranche must provide structured runtime diagnostics:

- live Admin stream for `Information` through `Critical`;
- local `Debug`/`Trace` by default;
- temporary remotely enabled Debug mode with automatic expiry;
- filters by application/component, machine, game profile, severity, event/category, correlation identifier and time;
- persistent local rotating logs as the fallback source when live forwarding is unavailable;
- secret redaction.

This requirement is recorded now but is not part of H1 domain implementation.

## Cross-cutting managed recovery requirement

Optional managed recovery checkpoints are now part of the target architecture for games that need them, especially the first Project Zomboid profile.

They remain parallel to normal synchronization:

- temporary checkpoints are not central versions;
- recovery is opt-in per profile;
- initial retention target is current + previous checkpoint;
- unclean session recovery is user-validated;
- local data is quarantined before checkpoint restoration;
- recovery material is removed only after confirmed usability and successful central promotion acknowledgement.

Implementation is intentionally deferred across the Agent/transfer/monitoring/lifecycle/storage/PZ tranches rather than pulled into H1.

See `docs/architecture/recovery-checkpoints.md`.

## Accepted persistence direction

The central server will use SQLite for GameSaveSync configuration and metadata. The active database lives locally on Succumbrae; Custodia receives safe backups rather than hosting the live SQLite file.

H1 defines persistence-independent domain objects only. H2.1A/H2.1B established the persistence modules, SQLite configuration and versioned migration foundation; H2.1C added the validated read-only lifecycle inspection contract; H2.1D added the validated fail-closed metadata readiness and safe-capability policy. Business schema/repositories remain upcoming H2 work.

See `docs/decisions/ADR-0001-server-metadata-sqlite.md`.
