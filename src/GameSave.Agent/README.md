# GameSave.Agent

Reserved for the Windows-side agent engine.

## Boundary

The agent may depend on Core and Contracts. UI concerns do not belong here, and the agent must not bypass the central-server architecture.

H0 contains no watcher, transfer, process-monitoring or Windows lifecycle behavior.
