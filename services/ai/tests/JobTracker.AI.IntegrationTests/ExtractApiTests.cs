using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobTracker.AI.IntegrationTests;

public sealed class ExtractApiTests : IClassFixture<AiApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public ExtractApiTests(AiApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private sealed record ExtractDto(string? Company, string? Position, object? Salary, string? Notes, string Model);

    [Fact]
    public async Task Extract_returns_200_with_fields()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/ai/extract",
            new { jobDescription = "Backend Engineer at Acme building .NET microservices." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ExtractDto>(Json);
        Assert.Equal("Acme", body!.Company);
        Assert.Equal("Backend Engineer", body.Position);
        Assert.Equal("stub-model", body.Model);
    }

    [Fact]
    public async Task Extract_empty_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/extract", new { jobDescription = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
