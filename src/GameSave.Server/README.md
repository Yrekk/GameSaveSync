# GameSave.Server

Central HTTP API host intended to run on Succumbrae.

## Boundary

The server is the ASP.NET Core host/composition root.

It depends on Application, Contracts and the concrete Persistence/Storage modules needed for dependency wiring. It must not become a second copy of domain logic, persistence logic or administrative orchestration.

Current H2 startup/composition behavior through H2.1E candidate:

- reads and validates the metadata database path;
- reads and validates the separate classification control-store path;
- registers Persistence in dependency injection, including the H2.1C inspection provider and H2.1E XML classification store;
- builds and runs the ASP.NET Core host;
- does **not** automatically inspect, open, create, migrate, restore or repair the metadata database.

There are currently:

- no business endpoint;
- no controller;
- no transport DTO;
- no profile/business persistence schema yet;
- no save-payload storage backend yet.

Administrative capabilities that could be invoked by Admin, CLI or future IA/tools belong to reusable `GameSave.Application` use cases/services. `Program.cs` remains bootstrap/composition only.

The committed Visual Studio launch profile is development-only. Production/LAN transport and security remain later work.
