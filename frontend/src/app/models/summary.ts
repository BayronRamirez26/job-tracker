/// Mirrors the AI service's contract at POST /api/ai/summarize.
/// The request/response field names match the C# records (camelCased on the wire).

export interface SummarizeRequest {
  jobDescription: string;
  candidateProfile?: string | null;
}

export interface SummaryResponse {
  summary: string;
  model: string;
}
