namespace JobTracker.AI.Application.Summaries.Dtos;

/// <summary>The generated summary and the model that produced it.</summary>
public sealed record SummaryResponse(string Summary, string Model);
