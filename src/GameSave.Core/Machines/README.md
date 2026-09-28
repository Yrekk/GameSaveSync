# Machine identity domain

This folder contains machine identity concepts that are independent from Windows, networking and Agent lifecycle.

`MachineId` identifies one logical GameSaveSync machine.

It is stable and opaque. Its internal representation is a non-empty `Guid`.

Core validates an existing identity but does not generate identifiers, register machines or decide whether a new Agent installation may reuse an existing identity.

The accepted generation direction is UUID v7 (`Guid.CreateVersion7()`) from the future Application/Server enrollment use case. Generation does not belong to Core, the Agent or SQLite.

Mutable values such as hostname, Windows username, workgroup/domain, IP address, paths, Agent version and last-seen timestamps are not identity.

A future Agent/Server enrollment workflow owns creation and authorized re-binding of machine identities. A new logical machine receives a new identity; a reformatted machine may retain an existing identity only through that explicit workflow.

See ADR-0008 for the accepted identity semantics.
