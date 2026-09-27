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
- Branch promotion policy is `feature/* → develop → deploy/succumbrae → main`.
- Starting with H1, validated feature work stops at `develop` unless Damien explicitly approves deployment promotion.
- The technical solution and central-server architecture remain the design baseline.

## IMPLEMENTED BUT NOT YET VALIDATED

H1.1 on `feature/h1-domain`:

- non-negative `SyncVersion` value object;
- local synchronization state with base version, dirty state and game-running state;
- central synchronization state;
- Core synchronization boundary documentation;
- unit tests for the new version invariant.

## DECIDED BUT NOT YET IMPLEMENTED

H1.2 will implement the deterministic decision table.

Conflict semantics are explicit and manual:

- divergent local/central evolution returns a conflict;
- timestamps do not choose a winner;
- the future UI must ask the user which side becomes authoritative;
- cancellation is allowed;
- both sides must be preserved before destructive resolution.

## TESTS / SMOKE

H1.1 CI and Damien's local validation are still pending.

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

Validate H1.1. If accepted, design H1.2's explicit decision result shape before implementing its decision table.
