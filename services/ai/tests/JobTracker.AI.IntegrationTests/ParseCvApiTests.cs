using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobTracker.AI.IntegrationTests;

public sealed class ParseCvApiTests : IClassFixture<AiApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public ParseCvApiTests(AiApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private sealed record ProfileDto(string? FullName, string? Headline, string[] Skills, string Model);

    [Fact]
    public async Task ParseCv_returns_200_with_profile()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/ai/parse-cv",
            new { cv = "\\documentclass{article}\\begin{document}Ada Lovelace, Software Engineer\\end{document}" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProfileDto>(Json);
        Assert.Equal("Ada Lovelace", body!.FullName);
        Assert.Contains("C#", body.Skills);
        Assert.Equal("stub-model", body.Model);
    }

    [Fact]
    public async Task ParseCv_empty_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/parse-cv", new { cv = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
