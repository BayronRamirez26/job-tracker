using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobTracker.AI.IntegrationTests;

public sealed class SummarizeApiTests : IClassFixture<AiApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public SummarizeApiTests(AiApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private sealed record SummaryDto(string Summary, string Model);

    [Fact]
    public async Task Summarize_returns_200_with_summary()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/ai/summarize",
            new { jobDescription = "Senior Backend Engineer. Build and operate .NET microservices on Azure." });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SummaryDto>(Json);
        Assert.False(string.IsNullOrWhiteSpace(body!.Summary));
        Assert.Equal("stub-model", body.Model);
    }

    [Fact]
    public async Task Summarize_empty_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/summarize", new { jobDescription = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
