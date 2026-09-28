# ADR-0006 — Save payload storage is a separate module with a local H2 backend

**Status:** Accepted  
**Date:** 27 September 2026

## Context

GameSaveSync has two fundamentally different persistence concerns:

1. server metadata/configuration;
2. game save payloads and versioned file artifacts.

Metadata belongs in the central database and is handled by `GameSave.Persistence`.

Save payloads are files and will eventually live on Custodia with transactional transfer/versioning behavior.

Putting both concerns into one Persistence module would create an infrastructure monolith and make later extraction harder.

H2 also needs enough local storage behavior to exercise the central Server before the real Agent, transfer protocol and Custodia integration exist.

## Decision

Introduce a separate `GameSave.Storage` project.

The Application layer owns storage capability ports.

`GameSave.Storage` provides concrete implementations.

Initial H2 implementation uses a controlled local filesystem backend on Succumbrae/dev storage.

Conceptually:

```text
GameSave.Core
        ↑
GameSave.Application
      ↑       ↑
      │       │
Persistence  Storage
 EF/SQLite   local files
      ↑       ↑
       \     /
        Server
```

## Responsibility split

### GameSave.Persistence

Owns metadata/configuration persistence:

- EF Core;
- SQLite;
- metadata entities/mappings;
- migrations;
- metadata repositories;
- metadata database snapshots/recovery.

### GameSave.Storage

Owns save-payload/file storage concerns:

- local filesystem backend in H2;
- later Custodia-backed implementation;
- safe mapping from logical GameSaveSync artifact identifiers to physical storage;
- low-level artifact existence/read/write/delete capabilities required by Application.

It does not own metadata queries or EF Core.

## Application-facing abstraction

Application must not receive a generic filesystem API.

Bad abstraction:

```text
WriteFile(path, bytes)
MoveDirectory(...)
DeleteAnything(path)
```

The port should speak in GameSaveSync storage concepts such as:

- profile;
- version/artifact identity;
- logical data root;
- relative artifact path;
- staging/final storage role when later required.

The physical path layout belongs to Storage implementations.

## H2 scope

H2 proves that the Server can persist and retrieve generic save artifacts through the storage abstraction.

H2 may include minimal capabilities such as:

- store an artifact;
- open/read an artifact;
- check artifact existence;
- controlled delete when required by a tested use case.

H2 does **not** implement:

- network upload protocol;
- resumable transfer;
- leases;
- full transactional version publication;
- distributed staging;
- managed recovery checkpoint capture;
- real SMB/Custodia integration;
- Project Zomboid-specific file semantics.

Those remain later tranche responsibilities.

## Future Custodia backend

H8 will introduce the real Custodia-backed storage implementation.

Application use cases should not need to change merely because:

```text
LocalFileStorage
→ CustodiaStorage
```

Provider-specific path rules remain inside `GameSave.Storage`.

## Extractability

`GameSave.Storage` is a distinct durable responsibility.

If another application later needs the same storage capability, the module can be:

- reused in-process;
- generalized only when a real second consumer appears;
- extracted behind an API/microservice when operational independence justifies it.

H2 does not create that microservice preemptively.

## Consequences

Benefits:

- metadata persistence and file storage cannot collapse into one infrastructure project;
- Custodia integration can evolve independently;
- Application remains storage-provider agnostic;
- local H2 testing is possible before Agent/H8;
- future extraction is materially easier.

Costs:

- another project and dependency boundary;
- artifact identity/path mapping requires deliberate design;
- H2 must resist pulling H5/H8 transfer/versioning behavior forward.
