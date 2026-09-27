# Tests

Tests mirror the architectural responsibilities under `src/`.

H0 only creates the test boundaries and runner configuration. It does not add placeholder business tests for behavior that does not exist.

Later tranches will progress from unit tests to component/integration tests, then synthetic filesystem sandboxes, and only afterwards explicit smoke tests against real infrastructure.
