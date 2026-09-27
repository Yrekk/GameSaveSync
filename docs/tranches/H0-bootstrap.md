# H0 — Bootstrap

**Status:** VALIDATED  
**Branch:** `feature/h0-bootstrap`

## Objective

Create a .NET 10 skeleton with clear boundaries, build/test infrastructure and documentation continuity.

H0 prepares the repository to **receive** future business logic. It does not implement or prototype that logic.

## In scope

- solution and project boundaries;
- dependency direction;
- shared build conventions;
- ASP.NET Core server-host boundary;
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
- business API endpoint or controller;
- transport DTO;
- desktop UI implementation;
- game-specific rule.

## Structural choices

### Projects exist before behavior

The five production projects reserve architectural locations without filling them with speculative classes.

### Server is an ASP.NET Core host

HTTP/API communication with the central server is already a validated architectural boundary, so H0 materializes `GameSave.Server` with `Microsoft.NET.Sdk.Web` and a minimal composition root.

This does not pull H2 forward: H0 defines no business route, controller, DTO or application behavior.

### Agent is Windows-targeted

The agent boundary targets Windows because that platform constraint is already part of the validated solution. This is a platform boundary, not synchronization behavior.

### UI framework is not selected

The UI project reserves a Windows UI boundary, but H0 does not commit to WPF or another framework. That choice belongs to the UI tranche.

### Tests exist without fake business assertions

Test projects are present so later tranches have an immediate home for tests. H0 does not invent placeholder domain behavior solely to make a test pass.

### Shared development launch configuration

The ASP.NET Core launch profile is versioned so a new workstation receives the same local development defaults instead of relying on per-machine IDE generation.

## Validation

Remote CI is green on the final H0 structure.

Damien completed the local validation sequence successfully:

```bash
dotnet restore GameSaveSync.sln
dotnet build GameSaveSync.sln --configuration Release --no-restore
dotnet test GameSaveSync.sln --configuration Release --no-build
git diff --check
git status --short
```

The test command reports the expected warnings that no tests are yet available in the intentionally empty H0 test projects. There are no failing tests.

`git diff --check` and `git status --short` are clean after validation.

## Final state

H0 is validated.

The repository now has the structural boundaries, shared development configuration, CI and documentation continuity needed to begin H1 without pulling H1 behavior into the bootstrap tranche.

## Next exact action

Do not merge or open H1 implicitly.

The next repository action requires Damien's explicit decision: merge the validated H0 branch to `main`, then start H1 from the agreed base.
