# GameSave.Agent

Reserved for the Windows-side agent engine.

## Boundary

The agent may depend on Core and Contracts. UI concerns do not belong here, and the agent must not bypass the central-server architecture.

Process monitoring, filesystem watching, transfer execution and Windows lifecycle behavior are not implemented yet; they arrive in later tranches.
