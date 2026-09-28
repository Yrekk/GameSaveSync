# Documentation

Documentation is maintained with the code.

## Project-specific documentation

- `architecture/` — GameSaveSync architectural boundaries and responsibilities.
- `tranches/` — living tranche tracking.
- `continuity/CURRENT_HANDOFF.md` — current recovery point between sessions.
- `decisions/` — GameSaveSync ADRs.
- `roadmap.md` — planned progression.
- `development/` — local pointers and GameSaveSync-specific workflow additions.

## Shared engineering documentation

Cross-project conventions are maintained in [NexusPrincipia](https://github.com/Yrekk/NexusPrincipia):

- Dev + AI operating model;
- project bootstrap rules;
- documentation conventions;
- handoff conventions;
- entrypoints and reusable administrative operations;
- C# / .NET and Python conventions;
- Debug & Observability reference;
- Database lifecycle, readiness and explicit administrative choice.

GameSaveSync should link to those documents rather than maintain divergent copies.

Stale documentation is a defect because it creates false context for future sessions.
