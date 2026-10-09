# Current handoff

**Date:** 9 October 2026  
**Repository:** `Yrekk/GameSaveSync`  
**Integration branch:** `develop`  
**Deployment branch:** `deploy/succumbrae`  
**Stable branch:** `main`  
**Current tranche:** H3 — Minimal Windows agent  
**State:** H2 accepted/merged; H3 scope accepted, implementation not started

Always verify the actual remote branch and HEAD before modifying the repository.

## Validated baseline

H2 is fully accepted and merged into `develop`.

Merge commit:

~~~text
6ec4406d68e562b94b5f90685224537e11e02401
~~~

Post-merge CI succeeded.

Final H2 validation before merge:

- Release build: 0 warnings / 0 errors;
- 140/140 tests passed remotely and locally;
- manual GET /api/system/status smoke test passed;
- targeted code review completed;
- Damien explicitly accepted H2.

No promotion to `deploy/succumbrae` or `main` has been authorized.

## H3 accepted scope

H3 creates the first real Windows Agent but deliberately does not synchronize game saves yet.

### H3.1 — Machine identity and enrollment

- minimum central machine registry;
- Server-authoritative UUID v7 MachineId creation;
- persistence/repository/migration;
- minimum enrollment transport boundary;
- mutable host metadata remains descriptive, never identity;
- rebind to an existing MachineId stays explicit/security-sensitive.

### H3.2 — Durable Agent state and Server client

- durable local Agent bootstrap state;
- persist/reload Server-assigned MachineId;
- typed Agent→Server client;
- consume central system status;
- enrollment only when no local identity exists;
- safe/diagnosable Server-unavailable behavior.

### H3.3 — Runtime and end-to-end handshake

- assemble Agent startup flow;
- derive Agent operational/readiness state;
- first run enrolls M1;
- second run reuses M1;
- no duplicate machine on restart;
- explicit end-to-end Server/DB/Agent test;
- Server-unavailable path tested.

Full tranche document:

`docs/tranches/H3-minimal-windows-agent.md`

## H3 design decision still required

Before implementing H3.1, decide the minimum trust model for enrollment.

The architecture must distinguish:

- creating a new logical MachineId;
- presenting an already assigned MachineId;
- explicitly rebinding to an existing logical machine.

Hostname, username or hardware heuristics are not identity proof.

## Development/review cadence to test on H3

Use only H3.1 / H3.2 / H3.3 as the default functional split.

Do not create H3.1A/H3.1B-style micro-tranches unless a genuinely independent architectural unit appears and Damien agrees the extra split is useful.

Before coding each lot:

1. explain the goal and meaningful internal checkpoints;
2. decide together whether adjacent H3 work should be grouped;
3. implement the agreed lot continuously;
4. keep CI/tests running;
5. do a targeted code review of the important/tricky mechanisms.

Routine review checks whether Damien still has the system map needed to orient development and catch missing operational states or wrong responsibilities. It is not a knowledge test on obscure SQL/SQLite/framework internals.

A separate exhaustive audit + documentation pass is planned before first V1.0 promotion from `deploy/succumbrae` to `main`.

If H3 validates this cadence, promote it to NexusPrincipia so H4 and later sessions inherit it.

## Future storage decisions captured after H2

ADR-0009 records the accepted future direction.

### Controlled destinations

Do not let a profile/artifact supply an arbitrary physical destination.

Use:

~~~text
logical StorageCategory
→ configured StorageTarget
→ provider-controlled physical root/layout
~~~

GameSaveSync currently needs the game-save use case only. Broader reusable categories such as application/document may exist later, but must not be implemented speculatively.

Target/category topology should be persisted/configurable rather than spread through hard-coded physical paths.

Application validates the logical contract; Storage independently verifies final physical root containment.

### Planned Custodia downtime

Custodia may intentionally be unavailable during scheduled periods. Current real-world example: approximately 00:00–08:00.

No downtime window is enabled by default.

Future StorageTarget configuration must be able to distinguish:

~~~text
planned temporary unavailability
!=
unexpected storage failure
~~~

Schedules require explicit timezone semantics.

H5 transfer logic must leave work safely pending across storage downtime. H8 materializes the actual Custodia target/provider and planned-availability configuration.

This reinforces the H2/ADR-0001 choice to keep active metadata SQLite local to Succumbrae: the control plane remains available and diagnosable while NAS payload storage is intentionally offline.

See:

`docs/decisions/ADR-0009-storage-targets-categories-availability.md`

## Next exact action

Start H3 from the accepted `develop` baseline.

Before coding H3.1:

1. verify current remote `develop` HEAD and CI;
2. create the H3 feature branch;
3. review `docs/tranches/H3-minimal-windows-agent.md`;
4. resolve the H3 enrollment trust model;
5. present H3.1 implementation checkpoints and decide the exact coding lot with Damien.

No code for H4/H5/H6/H7/H8 should be silently pulled into H3.
