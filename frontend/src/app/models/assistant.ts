/// Responses from the AI application assistant (/api/ai/fit, /cover-letter, /tailor-cv).

export interface FitResponse {
  score: number;
  strengths: string[];
  gaps: string[];
  summary: string;
  model: string;
}

export interface CoverLetterResponse {
  letter: string;
  model: string;
}

export interface TailoredCvResponse {
  markdown: string;
  model: string;
}
