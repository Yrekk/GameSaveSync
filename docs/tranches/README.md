# Development tranches

Each significant tranche has one living tracking document.

The document records scope, state, relevant decisions, validation and the next exact action. Long tranches update the same file instead of creating a new document for every micro-change.

## Current work

- H0 — validated.
- H1 — accepted and merged to `develop`.
- H2 — accepted and merged to `develop`.
- H3 — planned; scope accepted, implementation not started.
  - H3.1 — machine identity and enrollment.
  - H3.2 — durable local Agent state and typed Server client.
  - H3.3 — Agent runtime and end-to-end handshake.

Current H3 tracking document: [`H3-minimal-windows-agent.md`](H3-minimal-windows-agent.md).

The H3 split deliberately stops at H3.1/H3.2/H3.3 unless a genuinely independent architectural unit justifies another level and Damien explicitly agrees that the extra split is useful.
