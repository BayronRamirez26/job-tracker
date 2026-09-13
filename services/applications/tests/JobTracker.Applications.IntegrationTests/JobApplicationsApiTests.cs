using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobTracker.Applications.IntegrationTests;

/// <summary>
/// End-to-end tests over real HTTP against the API backed by a Testcontainers PostgreSQL.
/// The table is reset before each test (see <see cref="InitializeAsync"/>) for isolation.
/// </summary>
public sealed class JobApplicationsApiTests : IClassFixture<ApplicationsApiFactory>, IAsyncLifetime
{
    private const string Route = "/api/job-applications";
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    private readonly ApplicationsApiFactory _factory;
    private readonly HttpClient _client;

    public JobApplicationsApiTests(ApplicationsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    // Response shape for assertions. Enums are strings here, so no converter is needed to read them.
    private sealed record SalaryDto(decimal Min, decimal Max, string Currency);

    private sealed record AppDto(
        Guid Id, string Company, string Position, string Status, string Source,
        DateOnly? AppliedDate, string? Notes, SalaryDto? Salary,
        DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

    private static object ValidCreatePayload() => new
    {
        company = "Acme Corp",
        position = "Senior Backend Engineer",
        status = "Applied",
        source = "LinkedIn",
        appliedDate = "2026-09-01",
        notes = "Referred by a friend",
        salary = new { min = 90000, max = 120000, currency = "usd" }
    };

    [Fact]
    public async Task Post_creates_application_returns_201_and_location()
    {
        var response = await _client.PostAsJsonAsync(Route, ValidCreatePayload());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var created = await response.Content.ReadFromJsonAsync<AppDto>(Json);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal("Acme Corp", created.Company);
        Assert.Equal("Applied", created.Status);
        Assert.Equal("USD", created.Salary!.Currency); // normalized by the domain
    }

    [Fact]
    public async Task Get_by_id_returns_created_application()
    {
        var created = await CreateAsync();

        var response = await _client.GetAsync($"{Route}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var fetched = await response.Content.ReadFromJsonAsync<AppDto>(Json);
        Assert.Equal(created.Id, fetched!.Id);
    }

    [Fact]
    public async Task Get_all_returns_created_item()
    {
        await CreateAsync();

        var items = await _client.GetFromJsonAsync<List<AppDto>>(Route, Json);

        Assert.NotNull(items);
        Assert.Single(items!);
    }

    [Fact]
    public async Task Put_updates_status_and_returns_200()
    {
        var created = await CreateAsync();
        var update = new
        {
            company = created.Company,
            position = created.Position,
            status = "Interview",
            source = "LinkedIn",
            appliedDate = "2026-09-01",
            notes = "Onsite scheduled",
            salary = new { min = 100000, max = 130000, currency = "USD" }
        };

        var response = await _client.PutAsJsonAsync($"{Route}/{created.Id}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<AppDto>(Json);
        Assert.Equal("Interview", updated!.Status);
        Assert.True(updated.UpdatedAt >= created.UpdatedAt);
    }

    [Fact]
    public async Task Delete_removes_application_then_get_returns_404()
    {
        var created = await CreateAsync();

        var deleteResponse = await _client.DeleteAsync($"{Route}/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await _client.GetAsync($"{Route}/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Post_invalid_returns_400_with_field_errors()
    {
        var invalid = new
        {
            company = "",
            position = "Tester",
            status = "Applied",
            source = "Other",
            salary = new { min = 50000, max = 10000, currency = "US" }
        };

        var response = await _client.PostAsJsonAsync(Route, invalid);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = body.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Company", out _));
    }

    [Fact]
    public async Task Get_unknown_id_returns_404()
    {
        var response = await _client.GetAsync($"{Route}/{Guid.NewGuid()}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<AppDto> CreateAsync()
    {
        var response = await _client.PostAsJsonAsync(Route, ValidCreatePayload());
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AppDto>(Json))!;
    }
}
