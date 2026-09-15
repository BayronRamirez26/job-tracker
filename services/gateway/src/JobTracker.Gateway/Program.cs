var builder = WebApplication.CreateBuilder(args);

// YARP reverse proxy. Routes and clusters are declarative, loaded from the "ReverseProxy"
// section of configuration — the gateway holds no business logic, it only forwards requests
// (including the Authorization header) to the right service.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapReverseProxy();
app.MapHealthChecks("/health");

app.Run();
