# Development workflow

**Status:** active GameSaveSync project rule

The shared development operating model is maintained centrally in NexusPrincipia:

- [AI-assisted development operating model](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/ai-development-operating-model.md)
- [Project bootstrap](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/project-bootstrap.md)
- [Session continuity](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/session-continuity.md)
- [Documentation conventions](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/documentation-conventions.md)
- [C# / .NET conventions](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/languages/csharp-dotnet.md)

This file contains only **GameSaveSync-specific additions**.

## Start of a GameSaveSync session

Before substantial work:

1. read the root README;
2. read `docs/continuity/CURRENT_HANDOFF.md`;
3. read the active tranche document;
4. verify the actual remote branch and HEAD;
5. inspect the files relevant to the next action.

The validated technical solution and the dedicated GameSaveSync master prompt remain project-specific sources of truth.

## Branching policy

GameSaveSync uses:

```text
feature/*
   ↓
develop
   ↓
deploy/succumbrae
   ↓
main
```

- `feature/*` contains the active tranche/focused change.
- `develop` receives work that Damien has explicitly accepted.
- `deploy/succumbrae` is the deployment candidate.
- `main` represents the stable validated deployment state.

Starting with H1, the normal stopping point is `develop`.

No promotion to `deploy/succumbrae` or `main` occurs without Damien's explicit approval.

## GameSaveSync tranche tracking

Each significant tranche has a living document under `docs/tranches/` recording:

- objective and status;
- scope and explicit deferrals;
- technical decisions;
- commits;
- tests and smoke tests;
- known limits;
- next exact action.

Useful ideas outside the current scope go to the roadmap/backlog.

## Validation

Typical .NET validation:

```powershell
dotnet build GameSaveSync.sln --configuration Release
dotnet test GameSaveSync.sln --configuration Release --no-build
git diff --check
git status --short
```

Additional targeted or smoke tests are added when the tranche requires them.

CI success alone does not accept a tranche. Damien's local/explicit validation remains the final tranche gate.

## Project-specific Definition of Done

A GameSaveSync tranche is accepted only when:

- implementation matches the accepted scope;
- relevant checks are green;
- required smoke tests are green;
- tranche documentation is current;
- `CURRENT_HANDOFF.md` is current;
- Damien explicitly accepts the tranche.
