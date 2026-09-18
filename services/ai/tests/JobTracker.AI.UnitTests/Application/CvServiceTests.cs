using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Cv;
using JobTracker.AI.Application.Cv.Dtos;
using JobTracker.AI.Application.Cv.Validators;
using NSubstitute;

namespace JobTracker.AI.UnitTests.Application;

public class CvServiceTests
{
    private readonly IAiCompletionClient _ai = Substitute.For<IAiCompletionClient>();
    private readonly CvService _sut;

    public CvServiceTests()
    {
        _sut = new CvService(_ai, new ParseCvRequestValidator());
    }

    [Fact]
    public async Task ParseAsync_builds_a_structured_profile()
    {
        _ai.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AiCompletion(
                "{\"fullName\":\"Ada Lovelace\",\"headline\":\"Senior Engineer\",\"summary\":\"Builds systems.\"," +
                "\"location\":\"London\",\"yearsOfExperience\":8,\"skills\":[\"C#\",\"Angular\"]," +
                "\"experience\":[{\"company\":\"Analytical Engines\",\"title\":\"Lead\",\"period\":\"1840-1843\",\"highlights\":[\"First algorithm\"]}]," +
                "\"education\":[{\"institution\":\"Home\",\"degree\":null,\"year\":\"1833\"}],\"links\":[\"github.com/ada\"]}",
                "claude-opus-5"));

        var result = await _sut.ParseAsync(new ParseCvRequest("\\section{Experience} ..."));

        Assert.Equal("Ada Lovelace", result.FullName);
        Assert.Equal("Senior Engineer", result.Headline);
        Assert.Equal(8, result.YearsOfExperience);
        Assert.Equal(new[] { "C#", "Angular" }, result.Skills);
        Assert.Single(result.Experience);
        Assert.Equal("Analytical Engines", result.Experience[0].Company);
        Assert.Equal("First algorithm", result.Experience[0].Highlights[0]);
        Assert.Single(result.Education);
        Assert.Equal("claude-opus-5", result.Model);
    }

    [Fact]
    public async Task ParseAsync_defaults_missing_lists_to_empty()
    {
        _ai.CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new AiCompletion("{\"fullName\":\"Grace Hopper\"}", "claude-opus-5"));

        var result = await _sut.ParseAsync(new ParseCvRequest("Grace Hopper CV"));

        Assert.Equal("Grace Hopper", result.FullName);
        Assert.Empty(result.Skills);
        Assert.Empty(result.Experience);
        Assert.Empty(result.Education);
        Assert.Empty(result.Links);
    }

    [Fact]
    public async Task ParseAsync_throws_Validation_and_does_not_call_ai_when_empty()
    {
        await Assert.ThrowsAsync<ValidationException>(() => _sut.ParseAsync(new ParseCvRequest("")));
        await _ai.DidNotReceive().CompleteAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
