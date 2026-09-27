# GameSave.Server

Reserved for the central server application hosted on Succumbrae.

## Boundary

The server may depend on Core and Contracts. It must not become a second copy of domain logic.

H0 does not select an API surface, persistence mechanism or storage implementation and contains no server behavior.
