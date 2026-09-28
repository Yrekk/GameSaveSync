# Metadata migrations

This folder contains versioned EF Core migrations for the GameSaveSync metadata database.

## Operational rule

Migrations are authored during development, reviewed and committed to Git.

They are **never applied automatically by Server startup**.

Runtime/startup may inspect whether known migrations are pending. Applying them is an explicit administrative operation and will later run through the shared migration/snapshot/recovery coordinator.

The initial migration intentionally contains no business tables. It establishes migration history before H2.2 introduces real persisted domain data.
