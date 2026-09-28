# ADR-0002 — EF Core is the default persistence layer over SQLite

**Status:** Accepted  
**Date:** 27 September 2026

## Context

ADR-0001 established SQLite as the central metadata database for GameSaveSync.

H2 must now decide how the Server should access that database in a way that remains maintainable when the application grows.

The immediate H2 data model is small, so direct use of `Microsoft.Data.Sqlite` with handwritten SQL would be viable today.

The long-term requirements are broader:

- profile and machine metadata will grow;
- relationships and queries will become more complex;
- the Admin console will likely require richer filtering/search;
- schema changes are expected as the model evolves;
- a later provider/database-engine change must remain possible without coupling the Core domain to SQLite-specific code.

## Decision

GameSaveSync will use **EF Core as the default persistence abstraction with the SQLite provider**.

EF Core is not allowed to leak into `GameSave.Core`.

Persistence remains a Server infrastructure concern.

Conceptually:

```text
HTTP / application layer
        ↓
repository/application boundary
        ↓
EF Core persistence model
        ↓
SQLite provider
        ↓
SQLite
```

Domain objects and persistence entities remain separate when their responsibilities differ.

For example:

```text
GameSave.Core.GameProfile
        ↕ explicit mapping
Server persistence entities
        ↕
EF Core
        ↕
SQLite
```

## Hybrid rule

Choosing EF Core does not ban explicit SQL.

Raw SQL or direct provider access may be used for a narrowly justified operation when:

- generated SQL is unsuitable;
- a migration/data-repair operation is clearer in SQL;
- performance measurement demonstrates a real need;
- SQLite-specific functionality is required.

Such SQL must remain isolated in the persistence layer and must not appear in API handlers, `Program.cs`, or Core.

## Guardrails

- no lazy loading;
- no EF attributes or persistence concerns in Core domain types;
- no uncontrolled `DbContext` usage across the application;
- critical multi-step writes use explicit transactional boundaries;
- migrations are versioned and reviewed;
- repository/application boundaries prevent HTTP endpoints from becoming persistence scripts;
- provider-specific behavior is documented where it is intentionally relied upon.

## Maintainability rationale

The choice favors long-term maintainability over minimum code in H2.

EF Core provides stronger support for:

- schema evolution and migrations;
- relational mapping;
- query composition;
- refactoring persistence structure;
- future Admin querying;
- changing the underlying database provider with less application-layer disruption.

A provider change is **not** assumed to be automatic. SQLite-to-PostgreSQL or another engine would still require compatibility review, migrations and testing.

Likewise, consolidating metadata from several applications/databases into one shared database would remain an architectural/data-migration project. EF Core reduces coupling and migration effort but does not make such consolidation free or necessarily desirable.

## Database ownership

This ADR does not decide that all Succumbrae applications should share one physical database.

GameSaveSync owns its metadata schema.

If Succumbrae later moves toward a shared database server or consolidated operational platform, GameSaveSync should be able to migrate without Core changes, but cross-application schema ownership must remain explicit.

## Consequences

Benefits:

- lower long-term maintenance cost as relationships/schema grow;
- first-class migration tooling;
- easier refactoring of persistence shape;
- strong path toward richer Admin queries;
- keeps escape hatch for targeted SQL.

Costs:

- additional abstraction and package surface;
- developers must understand generated queries and EF tracking behavior;
- mapping/persistence entities add some code;
- provider migrations still require explicit engineering work.
