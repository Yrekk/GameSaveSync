# GameSave.Agent

Reserved for the Windows-side agent engine.

## Boundary

The agent may depend on Core and Contracts. UI concerns do not belong here, and the agent must not bypass the central-server architecture.

H3 gives this project its first real behavior.

Planned H3 responsibilities:

- durable local Agent bootstrap state;
- reuse of a Server-assigned MachineId across restarts;
- typed Agent→Server communication;
- enrollment when no identity exists;
- local operational/readiness reporting;
- safe behavior when the Server is unavailable.

The Agent does **not** create authoritative MachineId values by itself.

Process monitoring, filesystem watching, transfer execution, Windows lifecycle behavior and game-specific rules remain later-tranche responsibilities.

See `docs/tranches/H3-minimal-windows-agent.md`.
