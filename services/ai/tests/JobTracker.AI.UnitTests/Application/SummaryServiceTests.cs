using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Summaries;
using JobTracker.AI.Application.Summaries.Dtos;
using JobTracker.AI.Application.Summaries.Validators;
using NSubstitute;

namespace JobTracker.AI.UnitTests.Application;

public class SummaryServiceTests
{
    private readonly IAiCompletionClient _ai = Substitute.For<IAiCompletionClient>();
    private readonly SummaryService _sut;

    public SummaryServiceTests()
    {
        _sut = new SummaryService(_ai, new SummarizeRequestValidator());
    }

    [Fact]
    public async Task SummarizeAsync_returns_completion_text_and_model()
    {
        _ai.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AiCompletion("• A concise summary", "claude-opus-5"));

        var result = await _sut.SummarizeAsync(new SummarizeRequest("Build and operate .NET microservices."));

        Assert.Equal("• A concise summary", result.Summary);
        Assert.Equal("claude-opus-5", result.Model);
        // The job description is passed as the user prompt; the framing lives in the system prompt.
        await _ai.Received(1).CompleteAsync(
            Arg.Is<string>(system => system.Contains("summarize", StringComparison.OrdinalIgnoreCase)),
            Arg.Is<string>(user => user.Contains(".NET microservices")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SummarizeAsync_throws_Validation_and_does_not_call_ai_when_empty()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.SummarizeAsync(new SummarizeRequest("")));
        await _ai.DidNotReceive().CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
