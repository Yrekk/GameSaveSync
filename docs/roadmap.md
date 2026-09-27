# Roadmap

The validated technical solution defines the progression below.

| Tranche | Purpose | State |
| --- | --- | --- |
| H0 | Bootstrap, boundaries, CI and documentation system | Validated |
| H1 | Generic deterministic domain | In progress |
| H2 | Minimal central server | Planned |
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
