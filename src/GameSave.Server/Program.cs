var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// H0 establishes ASP.NET Core as the server transport host only.
// Business endpoints, controllers and synchronization behavior belong to later tranches.
app.Run();
