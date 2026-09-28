# Database

EF Core / SQLite metadata database infrastructure lives here.

## Current H2.1B state

- `GameSaveDbContext` exists but remains intentionally free of business tables.
- the DbContext and design-time EF factory are internal to Persistence;
- normal operational connections require an existing SQLite database;
- explicit first-time initialization has separate creation semantics;
- startup composes Persistence but does not open, create or migrate the DB.

Do not use `EnsureCreated()` for production schema management.

Versioned EF migrations are the accepted schema-evolution mechanism, and runtime migration execution is always an explicit administrative operation.
