# GameSave.Server

Central HTTP API host intended to run on Succumbrae.

## Boundary

The server is the ASP.NET Core host/composition root. It depends on Application, Contracts and the concrete Persistence/Storage modules needed for dependency wiring. It must not become a second copy of domain logic or a persistence implementation.

H0 deliberately materializes this project as an **ASP.NET Core host** because HTTP/API communication is already part of the validated architecture.

That does **not** mean H0 defines the future API surface. There are currently:

- no business endpoint;
- no controller;
- no transport DTO;
- no profile/business persistence schema yet;
- no save-payload storage backend yet.

The host exists so later tranches can add real application/API behavior without changing the fundamental server boundary.

The committed Visual Studio launch profile is development-only. It uses a stable localhost HTTP port and does not launch a browser; production/LAN transport and security choices remain outside H0.
