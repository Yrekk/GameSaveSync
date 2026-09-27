# Architecture

H0 defines **boundaries, not business rules**.

## Components

```text
Windows PC(s)
  GameSave.Agent
  GameSave.Agent.UI
        |
        v
GameSave.Server on Succumbrae
        |
        v
Custodia
```

The validated technical solution establishes a central-server architecture with NAS-backed storage. H0 only prepares the codebase so later tranches can implement that design safely.

## Project boundaries

| Project | Reserved responsibility | H0 rule |
| --- | --- | --- |
| GameSave.Core | Pure domain | No infrastructure dependency |
| GameSave.Contracts | Shared boundary contracts | No DTO until a real contract needs one |
| GameSave.Server | Central server application | No server behavior yet |
| GameSave.Agent | Windows-side engine | No monitoring or transfer behavior yet |
| GameSave.Agent.UI | Replaceable local UI | No business logic and no UI framework selected yet |

## Dependency direction

```text
Core       Contracts
  ^           ^
  |           |
  +--- Server |
  +--- Agent -+
         ^
         |
      Agent.UI
```

These references establish the places where later code can live without implementing that code prematurely.

## Documentation rule

When later architecture becomes non-obvious, document the **reason** near the relevant boundary. Obvious models and DTOs do not receive commentary merely to increase documentation volume.
