# Current handoff

**Date:** 27 September 2026  
**Repository:** `Yrekk/GameSaveSync`  
**Working branch:** `feature/h1-domain`  
**Integration branch:** `develop`  
**Deployment branch:** `deploy/succumbrae`  
**Stable branch:** `main`  
**Current tranche:** H1 — Generic deterministic domain

Always verify the actual remote branch and HEAD before modifying the repository.

## VALIDATED

- H0 bootstrap is validated and promoted through `main`.
- H1.1 is validated locally and remotely on `feature/h1-domain`:
  - non-negative `SyncVersion` value object;
  - local synchronization state with base version, dirty state, game-running state and integrity state;
  - central synchronization state;
  - fail-closed `SaveIntegrityState` vocabulary;
  - Core synchronization boundary documentation;
  - unit tests for the version invariant and default integrity state.
- Branch promotion policy is `feature/* → develop → deploy/succumbrae → main`.
- Starting with H1, validated feature work stops at `develop` unless Damien explicitly approves deployment promotion.
- The technical solution and central-server architecture remain the design baseline.

## IMPLEMENTED BUT NOT YET VALIDATED

None for H1.2 yet.

## DECIDED BUT NOT YET IMPLEMENTED

H1.2 will implement the deterministic decision table.

Safety requirements carried forward:

- a dirty save that is `Unknown`, `RequiresValidation` or `Invalid` must not be automatically promoted centrally;
- abnormal session termination can later move local integrity to `RequiresValidation`;
- the UI must tell the user to launch the game and verify the save, then accept **OK**, **KO**, or **later**;
- no response keeps synchronization blocked;
- divergent local/central evolution returns a conflict;
- timestamps do not choose a winner;
- both sides are preserved before destructive conflict resolution.

Crash/unclean-exit detection itself belongs to later Agent/lifecycle work. The future UI belongs to its own tranche.

Operational diagnostics are also decided but not implemented: structured logs feed console, local files and a filterable live Admin stream. Normal remote levels are Information through Critical; Debug/Trace stay local unless temporarily enabled from Admin with an automatic expiry.

## TESTS / SMOKE

H1.1 remote CI and Damien's local validation are green.

No filesystem, save, NAS, Windows process, network or synchronization transfer has been touched.

## KNOWN RISKS

The important H1 risk is semantic: ambiguous state or decision types could later make destructive synchronization behavior harder to reason about. Keep the domain small, explicit and deterministic.

## READ FIRST NEXT SESSION

1. root `README.md`;
2. this file;
3. `docs/tranches/H1-domain.md`;
4. `src/GameSave.Core/Synchronization/README.md`;
5. actual remote branch and HEAD.

## NEXT EXACT ACTION

Design H1.2's explicit decision result shape before implementing its decision table.
