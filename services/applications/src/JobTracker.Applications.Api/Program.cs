using System.Text.Json.Serialization;
using JobTracker.Applications.Api.Exceptions;
using JobTracker.Applications.Application;
using JobTracker.Applications.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Services: this is the composition root, the one place the layers are wired together ------

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize and accept enums by name ("Applied") instead of by number (4).
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Each layer owns its own registrations; the root just calls into them.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// RFC 7807 ProblemDetails responses + a single handler that maps exceptions to status codes.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

// --- HTTP pipeline ----------------------------------------------------------------------------

// Must be early so it can catch exceptions from everything downstream.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposed so the integration-test project can boot the app via WebApplicationFactory<Program>.
// (Top-level statements otherwise compile Program as an internal class the tests can't reference.)
public partial class Program { }
