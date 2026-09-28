# Continuity

`CURRENT_HANDOFF.md` remains the single GameSaveSync entry point for resuming development after a session change.

The shared handoff convention is maintained in NexusPrincipia:

[Session continuity and handoff](https://github.com/Yrekk/NexusPrincipia/blob/main/docs/development/session-continuity.md)

GameSaveSync keeps the actual current state locally because branch, tranche, tests, decisions and next action are project-specific.

Historical handoffs may be archived at meaningful milestones, but they must never compete with `CURRENT_HANDOFF.md` as the current recovery point.
