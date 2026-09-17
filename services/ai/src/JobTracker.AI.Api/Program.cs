using JobTracker.AI.Api.Exceptions;
using JobTracker.AI.Application;
using JobTracker.AI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Services: composition root (no database — this service is stateless) ----------------------
builder.Services.AddControllers();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

// --- HTTP pipeline ----------------------------------------------------------------------------
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
public partial class Program { }
