using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Extraction;
using JobTracker.AI.Application.Extraction.Dtos;
using JobTracker.AI.Application.Extraction.Validators;
using NSubstitute;

namespace JobTracker.AI.UnitTests.Application;

public class ExtractionServiceTests
{
    private readonly IAiCompletionClient _ai = Substitute.For<IAiCompletionClient>();
    private readonly ExtractionService _sut;

    public ExtractionServiceTests()
    {
        _sut = new ExtractionService(_ai, new ExtractRequestValidator());
    }

    [Fact]
    public async Task ExtractAsync_parses_structured_fields()
    {
        _ai.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AiCompletion(
                "{\"company\":\"Acme\",\"position\":\"Backend Engineer\",\"salary\":{\"min\":120000,\"max\":160000,\"currency\":\"USD\"},\"notes\":\"Builds .NET services.\"}",
                "claude-opus-5"));

        var result = await _sut.ExtractAsync(new ExtractRequest("Backend Engineer at Acme, 120-160k USD."));

        Assert.Equal("Acme", result.Company);
        Assert.Equal("Backend Engineer", result.Position);
        Assert.Equal(120000, result.Salary!.Min);
        Assert.Equal(160000, result.Salary.Max);
        Assert.Equal("USD", result.Salary.Currency);
        Assert.Equal("claude-opus-5", result.Model);
    }

    [Fact]
    public async Task ExtractAsync_tolerates_markdown_fenced_json()
    {
        _ai.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AiCompletion(
                "```json\n{\"company\":\"Globex\",\"position\":null,\"salary\":null,\"notes\":null}\n```",
                "claude-opus-5"));

        var result = await _sut.ExtractAsync(new ExtractRequest("Some posting."));

        Assert.Equal("Globex", result.Company);
        Assert.Null(result.Position);
        Assert.Null(result.Salary);
        Assert.Null(result.Notes);
    }

    [Fact]
    public async Task ExtractAsync_throws_Validation_and_does_not_call_ai_when_empty()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ExtractAsync(new ExtractRequest("")));
        await _ai.DidNotReceive().CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
