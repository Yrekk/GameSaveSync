# H3 — Minimal Windows agent

**Status:** Planned — scope accepted, implementation not started
**Branch:** to be created from develop when H3 starts
**Base:** accepted H2 on develop

## Objective

Make a real Windows-side Agent exist as a durable client of the central GameSaveSync server.

At the end of H3, an Agent installation must be able to:

- start on Windows;
- obtain a server-authoritative logical MachineId through an enrollment flow;
- persist the assigned identity locally;
- restart without silently becoming a second logical machine;
- contact the central Server through an explicit client boundary;
- report a usable local operational/readiness state;
- fail safely and remain diagnosable when the Server is unavailable.

H3 deliberately does not synchronize game saves yet.

## Explicitly deferred after H3

- H4 — Replaceable minimal UI: desktop UI over Agent capabilities and diagnostics.
- H5 — Reliable transactional transfers: PUSH/PULL, staging, validation, publication, acknowledgement and conflict-safe transfer behavior.
- H6 — Generic process/folder monitoring: process detection and filesystem observation.
- H7 — Windows lifecycle handling: shutdown/sleep/login/logout and clean/unclean session handling.
- H8 — Real Custodia storage: real NAS-backed payload target, storage-target configuration and scheduled storage availability.
- H9 — Project Zomboid: first concrete game profile/policy validation.

The Agent remains a generic engine. It does not contain Project Zomboid-specific rules.

## H3.1 — Machine identity and enrollment

### Goal

Materialize ADR-0008 now that a real Agent exists.

~~~text
fresh Agent
→ enrollment request
→ Server authorizes creation
→ Server creates UUID v7 MachineId
→ Server persists machine record
→ Agent receives assigned identity
~~~

Expected scope:

- introduce the minimum central machine registry required by the Agent;
- persist stable MachineId;
- persist only mutable descriptive metadata with a real H3 consumer;
- add the server-side Application use case and repository port;
- add Persistence mapping/migration;
- add the minimum transport contract/endpoint needed for enrollment;
- prevent an Agent from self-assigning an authoritative MachineId;
- keep rebind/re-enrollment to an existing MachineId explicit and security-sensitive.

Important invariant:

~~~text
MachineId = stable logical identity
hostname / username / paths / Agent version = mutable metadata
~~~

A fresh installation must never reclaim an existing MachineId merely because mutable host metadata happens to match.

### Principal checkpoint

A new machine can be enrolled and later retrieved centrally without identity ambiguity, with migration/repository/transport tests green.

## H3.2 — Durable local Agent state and Server client

### Goal

Give the Agent the minimum durable local state and network boundary required to survive restart and communicate with the Server.

~~~text
Agent local state
├── Server address/configuration
├── assigned MachineId
└── minimum bootstrap state
~~~

The local copy of MachineId is necessary for operation but is not the authority that creates the identity.

Expected scope:

- define a dedicated local Agent-state persistence boundary;
- persist/reload the assigned MachineId safely;
- define a typed Agent→Server client boundary instead of scattering raw HTTP calls;
- call the existing system-status endpoint;
- perform enrollment when no local identity exists;
- reuse the existing identity after restart;
- handle Server unavailable/timeouts without inventing a new identity or corrupting local state;
- expose enough diagnostic information for H4 to display later.

### Principal checkpoint

Restarting the Agent preserves the same MachineId, while a temporarily unavailable Server leaves the Agent coherent and diagnosable.

## H3.3 — Agent runtime and end-to-end handshake

### Goal

Assemble the H3 pieces into one real minimal Agent runtime.

Expected startup flow:

~~~text
Agent starts
→ loads local state
→ observes current mutable host metadata
→ contacts Server
→ enrolls only if no identity exists
→ checks central readiness
→ derives local Agent operational state
~~~

The Agent should be able to answer through one reusable status model:

- is local configuration usable?
- is a MachineId assigned?
- is the Server reachable?
- what central readiness is currently reported?
- is the enrollment/local identity state coherent?

### End-to-end validation target

~~~text
first Agent run
→ enrollment creates M1
→ Agent stores M1

Agent stops

second Agent run
→ reloads M1
→ no second logical machine is created
→ Server/Agent handshake succeeds
~~~

Also validate the Server-unavailable path.

### Principal checkpoint

The full bootstrap/restart/handshake flow works end-to-end and remains fail-safe when the Server is unreachable.

## Enrollment trust — decision required before implementation

Enrollment creates an authoritative identity, so a completely unauthenticated production-style endpoint is not an acceptable final architecture.

H3 does not need to implement the entire future authentication/authorization system, but implementation must explicitly decide the minimum trust model for initial enrollment.

The discussion must distinguish:

- creation of a new MachineId;
- an Agent presenting an already assigned MachineId;
- explicit security-sensitive rebind to an existing logical MachineId.

Hostname, username or hardware heuristics are never sufficient proof of identity.

## H3 implementation rhythm

H3 intentionally tests a more proportionate development cadence.

The tranche is split only into the three coherent functional checkpoints H3.1, H3.2 and H3.3.

Do not introduce H3.1A / H3.1B / ... unless a genuinely independent architectural unit appears and Damien explicitly agrees that the extra split is useful.

Before each implementation run:

1. present the sub-tranche objective;
2. present the important internal checkpoints/risks;
3. decide with Damien whether adjacent work should be grouped or kept separate;
4. code the agreed lot continuously;
5. keep tests/CI running during implementation;
6. perform a targeted code review of the mechanisms that matter;
7. validate before moving to the next coherent sub-tranche.

The purpose of the review is not to test obscure framework/database trivia. It is to ensure Damien still understands the system direction well enough to challenge missing states, incorrect responsibilities or product/operational assumptions.

Deep technology-specific explanation and exhaustive audit belong to the pre-V1 promotion audit rather than routine sub-tranche reviews.

## H3 acceptance

H3 is accepted only after:

- H3.1/H3.2/H3.3 functional checkpoints are complete;
- relevant Release build/tests are green;
- end-to-end Agent restart/enrollment handshake is validated;
- targeted code review is complete;
- documentation/handoff is current;
- Damien explicitly accepts H3.

No promotion beyond develop occurs without explicit authorization.
