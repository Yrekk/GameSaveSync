# H0 — Bootstrap

**Status:** CI GREEN — LOCAL VALIDATION PENDING  
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

GitHub Actions has successfully completed restore, build and test on the feature branch.

Local validation remains:

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

## Current state

The remote skeleton builds successfully in CI. H0 is not considered validated until Damien has pulled the branch, reviewed the structure and completed the local validation sequence.

## Next exact action

Damien pulls `feature/h0-bootstrap`, reviews the skeleton, then executes the local validation commands above.
