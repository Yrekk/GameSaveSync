# H0 — Bootstrap

**Status:** IMPLEMENTED — VALIDATION PENDING  
**Branch:** `feature/h0-bootstrap`

## Objective

Create a .NET 10 skeleton with clear boundaries, build/test infrastructure and documentation continuity.

H0 prepares the repository to **receive** future business logic. It does not implement or prototype that logic.

## In scope

- solution and project boundaries;
- dependency direction;
- shared build conventions;
- test project boundaries;
- CI;
- root and local architecture documentation;
- development workflow;
- current-handoff mechanism;
- roadmap.

## Explicitly out of scope

H0 contains no:

- game/profile domain model;
- synchronization decision;
- version/dirty-state logic;
- conflict logic;
- filesystem scanner;
- watcher;
- process detection;
- transfer implementation;
- NAS access;
- persistence choice;
- server API;
- desktop UI implementation;
- game-specific rule.

## Structural choices

### Projects exist before behavior

The five production projects reserve architectural locations without filling them with speculative classes.

### Agent is Windows-targeted

The agent boundary targets Windows because that platform constraint is already part of the validated solution. This is a platform boundary, not synchronization behavior.

### UI framework is not selected

The UI project reserves a Windows UI boundary, but H0 does not commit to WPF or another framework. That choice belongs to the UI tranche.

### Tests exist without fake business assertions

Test projects are present so later tranches have an immediate home for tests. H0 does not invent placeholder domain behavior solely to make a test pass.

## Validation

Run locally:

```bash
dotnet restore GameSaveSync.sln
dotnet build GameSaveSync.sln --configuration Release --no-restore
dotnet test GameSaveSync.sln --configuration Release --no-build
```

Then separately:

```bash
git diff --check
```

```bash
git status --short
```

CI runs the same restore/build/test sequence on the feature branch.

## Current state

Implementation is present on the feature branch but is not considered validated until CI and Damien's local checks are green.

## Next exact action

Inspect CI, then have Damien pull the branch and execute the validation sequence.
