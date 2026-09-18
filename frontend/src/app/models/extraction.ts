/// Mirrors the AI service contract at POST /api/ai/extract.

export interface ExtractRequest {
  jobDescription: string;
  candidateProfile?: string | null;
}

export interface ExtractedSalary {
  min: number;
  max: number;
  currency: string;
}

export interface ExtractionResponse {
  company: string | null;
  position: string | null;
  salary: ExtractedSalary | null;
  notes: string | null;
  model: string;
}
