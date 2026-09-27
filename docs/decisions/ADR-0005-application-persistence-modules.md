# ADR-0005 — Application and Persistence are separate extractable modules

**Status:** Accepted  
**Date:** 27 September 2026

## Context

GameSaveSync is expected to grow across several entry points and infrastructure concerns:

- ASP.NET Core Server;
- desktop/local client workflows;
- future Web Admin;
- persistence;
- snapshots/recovery;
- later Custodia-backed storage;
- possible future services shared with other applications.

The project philosophy is to avoid a monolithic Server and keep responsibilities separable enough that a capability can later be extracted into a service when a second real consumer appears.

The goal is not to build premature microservices.

The goal is to avoid coupling that would make future extraction expensive.

## Decision

H2 introduces two distinct projects:

- `GameSave.Application`
- `GameSave.Persistence`

They remain part of the same deployable system for now.

### GameSave.Application

Owns application use cases and the ports they require.

Examples:

- profile management use cases;
- migration coordination;
- snapshot/recovery coordination;
- application-level operation results;
- repository abstractions such as `IProfileRepository`;
- snapshot/persistence capability abstractions required by use cases.

Application may depend on Core.

Application must not depend on EF Core, SQLite, ASP.NET Core, desktop UI technology, Web Admin technology or NAS-specific infrastructure.

### GameSave.Persistence

Owns concrete metadata persistence.

Examples:

- EF Core `DbContext`;
- persistence entities;
- EF mappings/configurations;
- migrations;
- SQLite repository implementations;
- SQLite-safe metadata snapshot implementation;
- provider-specific SQL when explicitly justified.

Persistence may depend on Application to implement its ports and on Core where mapping requires domain construction.

Persistence must not own HTTP endpoints, UI, or application orchestration.

### GameSave.Server

Remains a thin host/composition root.

It may:

- configure dependency injection;
- configure ASP.NET Core;
- expose HTTP/administrative adapters;
- invoke Application use cases;
- bind concrete Persistence implementations.

It must not become the place where repository logic, EF queries or synchronization domain logic accumulates.

### GameSave.Contracts

Remains reserved for real cross-process/network contracts.

It does not become the home of repository interfaces or shared internal models.

## Dependency direction

Target H2 dependency direction:

```text
GameSave.Core
      ↑
GameSave.Application
      ↑
GameSave.Persistence

GameSave.Server
   ├── depends on Application
   ├── depends on Persistence for composition
   └── depends on Contracts for transport

GameSave.Contracts
   └── transport/shared boundary contracts only
```

A concrete implementation may require Server and Persistence to both reference Application while Application references Core.

The important invariant is that Core and Application remain independent from infrastructure frameworks.

## Extractability principle

Modules are designed so future extraction is possible without requiring it today.

If another application later needs the same capability, options include:

- referencing a reusable library when in-process reuse is appropriate;
- extracting the capability behind an API;
- promoting it to an independent microservice when operational ownership and reuse justify that cost.

No module is made artificially generic before a real second consumer exists.

Extraction should be a packaging/deployment change around a stable responsibility, not a rewrite caused by hidden coupling.

## Why not one generic Infrastructure project?

A single `GameSave.Infrastructure` project would invite unrelated technical concerns to accumulate over time.

Persistence is a durable responsibility and therefore gets its own project now.

Future concerns such as Custodia-backed save storage should receive their own module/project only when that responsibility becomes real.

## Repository placement

Repository interfaces belong to `GameSave.Application` because they express capabilities required by use cases.

Concrete EF/SQLite repositories belong to `GameSave.Persistence`.

Example:

```text
GameSave.Application
└── IProfileRepository
        ↑
        │ implemented by
        │
GameSave.Persistence
└── EfProfileRepository
```

This keeps application logic independent from the chosen provider.

## Consequences

Benefits:

- Server remains small;
- persistence can evolve independently;
- application use cases are reusable from multiple entry points;
- future extraction into libraries/services is easier;
- framework dependencies stay at the edges;
- testing use cases without SQLite or HTTP becomes straightforward.

Costs:

- more projects/references;
- explicit mapping and dependency wiring;
- discipline is required to prevent circular references;
- some abstractions may remain internal until a second implementation exists.

The project count is justified by durable responsibility boundaries, not by a desire to maximize modularity mechanically.
