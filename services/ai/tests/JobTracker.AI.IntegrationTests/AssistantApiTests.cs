using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace JobTracker.AI.IntegrationTests;

public sealed class AssistantApiTests : IClassFixture<AiApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public AssistantApiTests(AiApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    private static object Body() => new { jobDescription = "Backend engineer at Acme.", candidateProfile = "Senior engineer, C#." };

    private sealed record CoverLetterDto(string Letter, string Model);
    private sealed record TailoredCvDto(string Content, string Format, string Model);
    private sealed record FitDto(int Score, string[] Strengths, string[] Gaps, string Summary, string Model);

    [Fact]
    public async Task CoverLetter_returns_200_with_text()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/cover-letter", Body());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CoverLetterDto>(Json);
        Assert.Contains("Hiring Manager", body!.Letter);
        Assert.Equal("stub-model", body.Model);
    }

    [Fact]
    public async Task TailorCv_returns_200_with_markdown()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/tailor-cv", Body());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TailoredCvDto>(Json);
        Assert.StartsWith("# Ada Lovelace", body!.Content);
        Assert.Equal("markdown", body.Format);
    }

    [Fact]
    public async Task TailorCv_latex_returns_200_with_latex_source()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/ai/tailor-cv",
            new { jobDescription = "Backend engineer at Acme.", candidateProfile = "Senior engineer, C#.", format = "latex" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TailoredCvDto>(Json);
        Assert.StartsWith("\\documentclass", body!.Content);
        Assert.Equal("latex", body.Format);
    }

    [Fact]
    public async Task Fit_returns_200_with_a_score()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/fit", Body());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<FitDto>(Json);
        Assert.Equal(78, body!.Score);
        Assert.Contains("Strong C# background", body.Strengths);
        Assert.Contains("No Kubernetes", body.Gaps);
    }

    [Fact]
    public async Task Fit_empty_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/ai/fit", new { jobDescription = "" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
