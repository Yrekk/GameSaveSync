# Development workflow

**Status:** active project rule

## Sources of truth

When information conflicts:

1. current code on the working branch;
2. Damien's latest explicit decision;
3. current handoff;
4. current project documentation;
5. validated technical solution;
6. project master prompt;
7. adapted collaboration conventions;
8. historical conversations.

A significant contradiction is reported rather than silently reconciled.

## Start of a session

Before substantial work:

1. read the root README;
2. read `docs/continuity/CURRENT_HANDOFF.md`;
3. read the active tranche document;
4. verify the actual remote branch and HEAD;
5. inspect the files relevant to the next action.

If Damien says he pushed changes, re-read the remote HEAD before continuing.

## Feature workflow

```text
feature branch
→ code + tests + documentation
→ remote branch update
→ Damien pulls locally
→ brief explaining responsibilities and changes
→ targeted/full tests as relevant
→ smoke tests when relevant
→ corrections on the feature branch
→ merge only after explicit acceptance
```

## Tranches

Each significant tranche has one living tracking document recording:

- objective and status;
- chosen technical approach;
- sub-slices when useful;
- decisions;
- relevant commits;
- tests and smoke tests;
- known limits;
- next exact action.

Do not grow the scope silently. Useful non-blocking ideas go to the roadmap/backlog.

## Documentation standard

Documentation is part of the Definition of Done.

Comment and document **why** when it is useful:

- architectural responsibility;
- invariant;
- non-obvious contract;
- ordering/safety reason;
- important side effect;
- deliberate trade-off.

Do not comment what names and types already make obvious.

A simple DTO or model needs no essay. Explain its representation only when the choice matters, for example because of compatibility, serialization, security, immutability or a deliberate separation between domain and transport types.

Meaningful subsystem folders receive a concise README explaining what belongs there and what does not. Trivial folders do not receive artificial documentation.

## Handoff

`docs/continuity/CURRENT_HANDOFF.md` is the single current recovery entry point.

It separates:

- **VALIDATED**;
- **IMPLEMENTED BUT NOT YET VALIDATED**;
- **DECIDED BUT NOT YET IMPLEMENTED**.

The handoff explains state; Git proves what code exists.

## Definition of Done

A tranche is validated only when:

- implementation matches its agreed scope;
- relevant automated checks are green;
- required smoke tests are green;
- documentation is current;
- tranche tracking is current;
- current handoff is current;
- Damien explicitly accepts the tranche.

Passing CI alone is not acceptance.
