using System.Text;
using System.Text.Json.Serialization;
using JobTracker.Applications.Api.Exceptions;
using JobTracker.Applications.Application;
using JobTracker.Applications.Infrastructure;
using JobTracker.Applications.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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

// --- JWT bearer authentication ----------------------------------------------------------------
// Validates the SAME token the Users service issues — same signing key / issuer / audience,
// injected via configuration (env vars), so the services stay autonomous but trust one token.
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = jwt["SigningKey"];
if (string.IsNullOrWhiteSpace(signingKey))
{
    throw new InvalidOperationException(
        "Jwt:SigningKey is not configured. Set it via user-secrets or the Jwt__SigningKey environment variable.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep standard JWT claim names (e.g. "sub") instead of ASP.NET's default URI remapping.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the token from the Users service's /api/auth/login.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Apply EF Core migrations on startup when enabled — used by the orchestrated Docker Compose so
// `docker compose up` initializes the schema. Off for local `dotnet run` (manual `dotnet ef`).
if (app.Configuration.GetValue<bool>("RunMigrationsAtStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationsDbContext>();
    await db.Database.MigrateAsync();
}

// --- HTTP pipeline ----------------------------------------------------------------------------

// Must be early so it can catch exceptions from everything downstream.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Exposed so the integration-test project can boot the app via WebApplicationFactory<Program>.
// (Top-level statements otherwise compile Program as an internal class the tests can't reference.)
public partial class Program { }
