# Source projects

The source tree is organized by architectural boundary.

H0 defines the containers and allowed dependency direction only. It implements no synchronization behavior.

- **GameSave.Core** — future pure domain layer.
- **GameSave.Contracts** — future shared boundary contracts.
- **GameSave.Server** — future central server application.
- **GameSave.Agent** — future Windows-side engine.
- **GameSave.Agent.UI** — future replaceable local UI adapter.

Project-local READMEs describe boundaries without inventing behavior that does not exist yet.
