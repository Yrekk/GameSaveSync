# Tests

Tests mirror the architectural responsibilities under `src/`.

Current validated H2.1B coverage includes focused Core, Application, Persistence and Storage tests.

Persistence tests protect, among other things:

- metadata path validation;
- operational connections not creating a missing DB;
- explicit initialization semantics;
- baseline migration pending/applied behavior;
- EF infrastructure remaining non-public;
- isolated SQLite test pooling.

The reviewed H2.1B code executed **68 passing tests** remotely:

- Core: 57;
- Application: 1;
- Persistence: 9;
- Storage: 1.

`GameSave.Server.Tests` and `GameSave.IntegrationTests` still intentionally contain no placeholder tests because no real Server/transport integration behavior exists yet.

Later tranches progress toward Server/component/integration tests, synthetic filesystem sandboxes and explicit smoke tests against real infrastructure.
