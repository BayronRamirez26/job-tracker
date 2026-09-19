using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using JobTracker.AI.Application.Abstractions;
using JobTracker.AI.Application.Cv.Dtos;
using JobTracker.AI.Application.Exceptions;

namespace JobTracker.AI.Application.Cv;

/// <summary>
/// Reads a candidate's CV (plain text or LaTeX source) and builds a structured professional
/// profile. Like the other AI use cases it asks the model for strict JSON and parses it here,
/// keeping the provider detail behind <see cref="IAiCompletionClient"/>.
/// </summary>
public sealed class CvService : ICvService
{
    private const string SystemPrompt =
        "You read a candidate's CV / resume (which may be LaTeX source) and build their " +
        "professional profile. Return ONLY a JSON object — no markdown fences, no commentary — " +
        "with exactly these keys: " +
        "\"fullName\" (string or null), " +
        "\"headline\" (a short professional title like \"Senior Backend Engineer\", or null), " +
        "\"summary\" (a 2-3 sentence professional summary, or null), " +
        "\"location\" (string or null), " +
        "\"yearsOfExperience\" (integer or null), " +
        "\"skills\" (array of skill/technology strings, [] if none), " +
        "\"experience\" (array of {\"company\": string, \"title\": string or null, " +
        "\"period\": string or null, \"highlights\": array of strings}, [] if none), " +
        "\"education\" (array of {\"institution\": string, \"degree\": string or null, " +
        "\"year\": string or null}, [] if none), " +
        "\"certifications\" (array of {\"name\": string, \"issuer\": string or null, " +
        "\"year\": string or null}, [] if none), and " +
        "\"links\" (array of URL strings, [] if none). " +
        "Extract only what the CV states; never invent facts. Use null or [] for anything absent.";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
    };

    private readonly IAiCompletionClient _aiCompletionClient;
    private readonly IValidator<ParseCvRequest> _validator;

    public CvService(IAiCompletionClient aiCompletionClient, IValidator<ParseCvRequest> validator)
    {
        _aiCompletionClient = aiCompletionClient;
        _validator = validator;
    }

    public async Task<CvProfileResponse> ParseAsync(ParseCvRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);

        var completion = await _aiCompletionClient.CompleteAsync(SystemPrompt, request.Cv.Trim(), cancellationToken);

        var parsed = Parse(completion.Text);

        return new CvProfileResponse(
            parsed.FullName,
            parsed.Headline,
            parsed.Summary,
            parsed.Location,
            parsed.YearsOfExperience,
            parsed.Skills ?? Array.Empty<string>(),
            parsed.Experience ?? Array.Empty<CvExperience>(),
            parsed.Education ?? Array.Empty<CvEducation>(),
            parsed.Certifications ?? Array.Empty<CvCertification>(),
            parsed.Links ?? Array.Empty<string>(),
            completion.Model);
    }

    private static ParsedProfile Parse(string text)
    {
        var json = IsolateJsonObject(text);
        try
        {
            return JsonSerializer.Deserialize<ParsedProfile>(json, JsonOptions) ?? Empty;
        }
        catch (JsonException)
        {
            throw new AiUnavailableException("The AI returned an unexpected format. Please try again.");
        }
    }

    private static string IsolateJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start < 0 || end <= start)
        {
            throw new AiUnavailableException("The AI did not return a profile.");
        }

        return text[start..(end + 1)];
    }

    private static readonly ParsedProfile Empty = new(null, null, null, null, null, null, null, null, null, null);

    private sealed record ParsedProfile(
        string? FullName,
        string? Headline,
        string? Summary,
        string? Location,
        int? YearsOfExperience,
        IReadOnlyList<string>? Skills,
        IReadOnlyList<CvExperience>? Experience,
        IReadOnlyList<CvEducation>? Education,
        IReadOnlyList<CvCertification>? Certifications,
        IReadOnlyList<string>? Links);
}
