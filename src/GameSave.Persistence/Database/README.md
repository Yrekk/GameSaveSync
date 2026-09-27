# Database

EF Core database infrastructure lives here.

`GameSaveDbContext` is intentionally empty in H2.1A: no business table exists until a real persistence use case defines its schema.

Do not use `EnsureCreated()` for production schema management. Versioned EF migrations are the accepted schema-evolution mechanism.
