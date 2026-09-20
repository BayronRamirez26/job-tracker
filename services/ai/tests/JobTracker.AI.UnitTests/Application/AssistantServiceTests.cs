using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Assistant;
using JobTracker.AI.Application.Assistant.Dtos;
using JobTracker.AI.Application.Assistant.Validators;
using JobTracker.AI.Application.Exceptions;
using NSubstitute;

namespace JobTracker.AI.UnitTests.Application;

public class AssistantServiceTests
{
    private readonly IAiCompletionClient _ai = Substitute.For<IAiCompletionClient>();
    private readonly AssistantService _sut;

    public AssistantServiceTests()
    {
        _sut = new AssistantService(_ai, new AssistRequestValidator());
    }

    private static AssistRequest Request() => new("Backend role at Acme.", "Senior engineer, 8y, C#.");

    private void AiReturns(string text) =>
        _ai.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AiCompletion(text, "claude-opus-5"));

    [Fact]
    public async Task CoverLetterAsync_returns_the_trimmed_letter_text()
    {
        AiReturns("  Dear Hiring Manager, ... Sincerely, Ada.  ");

        var result = await _sut.CoverLetterAsync(Request());

        Assert.Equal("Dear Hiring Manager, ... Sincerely, Ada.", result.Letter);
        Assert.Equal("claude-opus-5", result.Model);
    }

    [Fact]
    public async Task TailoredCvAsync_returns_markdown_by_default()
    {
        AiReturns("# Ada Lovelace\n\n## Summary\nTailored.");

        var result = await _sut.TailoredCvAsync(Request());

        Assert.StartsWith("# Ada Lovelace", result.Content);
        Assert.Equal("markdown", result.Format);
        Assert.Equal("claude-opus-5", result.Model);
    }

    [Fact]
    public async Task TailoredCvAsync_returns_latex_when_requested_and_strips_fences()
    {
        AiReturns("```latex\n\\documentclass{article}\\begin{document}x\\end{document}\n```");

        var result = await _sut.TailoredCvAsync(new AssistRequest("JD", "profile", "latex"));

        Assert.StartsWith("\\documentclass", result.Content); // fence removed
        Assert.DoesNotContain("```", result.Content);
        Assert.Equal("latex", result.Format);
    }

    [Fact]
    public async Task FitAsync_parses_the_score_strengths_and_gaps()
    {
        AiReturns("{\"score\":82,\"strengths\":[\"C# depth\"],\"gaps\":[\"No K8s\"],\"summary\":\"Strong fit.\"}");

        var result = await _sut.FitAsync(Request());

        Assert.Equal(82, result.Score);
        Assert.Equal(new[] { "C# depth" }, result.Strengths);
        Assert.Equal(new[] { "No K8s" }, result.Gaps);
        Assert.Equal("Strong fit.", result.Summary);
    }

    [Fact]
    public async Task FitAsync_clamps_out_of_range_scores()
    {
        AiReturns("{\"score\":140,\"strengths\":[],\"gaps\":[],\"summary\":\"x\"}");

        var result = await _sut.FitAsync(Request());

        Assert.Equal(100, result.Score);
    }

    [Fact]
    public async Task FitAsync_throws_AiUnavailable_on_unparseable_output()
    {
        AiReturns("not json at all");

        await Assert.ThrowsAsync<AiUnavailableException>(() => _sut.FitAsync(Request()));
    }

    [Fact]
    public async Task Validates_and_does_not_call_ai_when_job_description_is_empty()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.CoverLetterAsync(new AssistRequest("", "profile")));
        await _ai.DidNotReceive().CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
