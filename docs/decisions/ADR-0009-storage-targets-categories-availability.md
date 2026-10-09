# ADR-0009 — Storage destinations use configured targets, logical categories and explicit availability policy

**Status:** Accepted direction — implementation deferred to the tranche with the first real consumer
**Date:** 9 October 2026

## Context

The real GameSaveSync storage target will eventually be Custodia rather than the H2 local-filesystem backend.

A physical destination must not be supplied ad hoc by each profile or operation. Free-form destination paths would allow inconsistent placement, accidental cross-category storage and infrastructure details to leak into Application/business configuration.

There is also a real operational constraint: Custodia may be intentionally unavailable during scheduled periods. The current installation typically powers the NAS down from approximately 00:00 to 08:00.

A planned storage outage is not the same thing as an unexpected storage failure.

The metadata SQLite database deliberately remains local to Succumbrae, so the Server can remain operational and explain storage state even while Custodia is intentionally offline.

## Decision

Model physical storage destination separately from the logical classification of the data being stored.

~~~text
backup/profile/artifact
        ↓
logical StorageCategory
        ↓
configured StorageTarget
        ↓
GameSave.Storage provider
        ↓
controlled physical path
~~~

### StorageTarget

A future persisted target represents a concrete storage backend/configuration.

Potential metadata includes only fields justified by the real provider/use case, for example:

~~~text
StorageTarget
-------------
Id
DisplayName
Provider
Root
Enabled
AvailabilityPolicy
~~~

For Custodia this may eventually describe the NAS-backed storage provider/root. Provider-specific path/mount/SMB details remain infrastructure configuration and must not leak into Core business rules.

### StorageCategory

A logical category describes what kind of data is being routed independently from the physical target.

~~~text
StorageCategory
---------------
Id
Code
DisplayName
StorageTargetId
RelativeRoot
Enabled
~~~

Possible broader reusable categories could include game, application or document.

GameSaveSync currently needs only the game-save use case. Do not implement unrelated categories merely to make the system look generic.

Initial categories may be seeded for convenience, but the routing/topology should be persisted/configurable rather than scattered as hard-coded physical paths.

### Controlled routing

A caller chooses/owns the logical classification of the object, not an arbitrary physical destination.

~~~text
category = game
profile = project-zomboid
→ configured target/category mapping
→ controlled profile layout under the game root
~~~

A game artifact cannot redirect itself into another configured category/root through a user-supplied destination.

The existing H2 safe-relative-path rules remain part of this defense. Storage implementations must still verify that the fully resolved physical path remains under the configured category/target root.

This preserves the same defense-in-depth principle used in H2:

- Application validates the logical contract;
- Storage independently validates the resolved infrastructure path.

## Planned availability

A StorageTarget may later define optional planned-unavailability windows.

No downtime schedule is enabled by default.

A configured example for the current Custodia installation may be:

~~~text
00:00 → 08:00
timezone: explicit site timezone
~~~

The exact persistence model is deferred until H5/H8 has the real consumer, but the model must distinguish:

~~~text
storage unavailable inside configured planned window
→ expected temporary unavailability

storage unavailable outside configured planned window
→ unexpected failure / diagnostic condition
~~~

The schedule must use an explicit timezone rather than assuming server-local wall-clock semantics.

## Transfer consequence

H5 reliable transfers must tolerate temporary storage unavailability.

If an artifact becomes eligible for upload while its target is intentionally unavailable:

~~~text
operation ready
→ target intentionally unavailable
→ operation remains safely pending
→ no false success
→ no loss of local/recovery material
→ retry/resume when storage becomes available
~~~

A planned outage must not be treated as a completed transfer or as permission to discard the only safe local state.

## H8 consequence

H8 owns the real Custodia provider and therefore is expected to materialize the target/category routing and planned-availability configuration when the real use case exists.

The implementation may refine the exact tables/objects, but must preserve these invariants:

- no arbitrary free-form physical destination per profile/artifact;
- logical classification is separate from physical target;
- category/root containment is enforced;
- planned and unexpected unavailability are distinguishable;
- metadata authority remains available independently of NAS availability;
- Application remains provider-agnostic.

## Why the active metadata DB remains local

This operational requirement reinforces ADR-0001.

~~~text
Succumbrae Server     available
metadata SQLite       available
Custodia storage      planned unavailable
~~~

The central Server can still inspect metadata, expose diagnostics, keep pending work/state and explain that the storage outage is expected.

Hosting the active metadata DB on the NAS would couple control-plane availability to payload-storage availability and would defeat that behavior.

## Consequences

Benefits:

- storage routing remains deterministic and centrally controlled;
- physical topology can change without rewriting business profiles;
- data classes cannot accidentally escape into another configured root;
- planned NAS power-off can be modeled without generating false failure semantics;
- H5 can preserve pending work safely across storage downtime;
- H8 can introduce Custodia without hard-coding one physical layout into Application.

Costs:

- target/category metadata introduces additional configuration and validation;
- availability windows require timezone-aware scheduling semantics;
- Storage status will eventually need richer semantics than the H2 Ready / Unavailable foundation.

Do not enrich the H2 status model preemptively. Add the richer model when H5/H8 provides the first real consumer.
